using TurnosApp.Core.Application.DTOs.Turnos;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Domain.Exceptions;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class TurnoAppService : ITurnoAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly SolapamientoValidator _solapamientoValidator;
    private readonly INotificacionAppService _notificacionAppService;

    public TurnoAppService(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        SolapamientoValidator solapamientoValidator,
        INotificacionAppService notificacionAppService)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _solapamientoValidator = solapamientoValidator;
        _notificacionAppService = notificacionAppService;
    }

    public async Task<IReadOnlyList<TurnoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var turnos = await _unitOfWork.Turnos.GetAllConDetallesAsync(cancellationToken);

        // Sin VerAgendaCompleta, el usuario queda acotado a los turnos de su propio Recurso vinculado.
        var permisos = await _currentUserService.GetCurrentPermisosAsync(cancellationToken);
        if (!permisos.HasFlag(Permiso.VerAgendaCompleta))
        {
            var recursoId = await _currentUserService.GetCurrentRecursoIdAsync(cancellationToken);
            turnos = turnos.Where(t => t.RecursoId == recursoId).ToList();
        }

        return turnos
            .Where(t => t.Estado != EstadoTurno.Cancelado)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<TurnoDto> CrearTurnoAsync(CrearTurnoDto dto, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // ── Paso 0: Resolver cliente (existente o inline) — solo aplica al flujo de staff,
        // el self-service ya conoce su ClienteId por el JWT (ver CrearTurnoPublicoAsync). ──
        int clienteId;

        if (dto.ClienteId is not null)
        {
            var clienteExistente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId.Value, cancellationToken);

            if (clienteExistente is null)
                throw new NotFoundException(nameof(Cliente), dto.ClienteId.Value);

            clienteId = clienteExistente.Id;
        }
        else if (dto.ClienteNuevo is not null)
        {
            var clienteNuevo = new Cliente
            {
                Nombre = dto.ClienteNuevo.Nombre,
                Apellido = dto.ClienteNuevo.Apellido,
                Telefono = dto.ClienteNuevo.Telefono,
                Activo = true
            };

            await _unitOfWork.Clientes.AddAsync(clienteNuevo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            clienteId = clienteNuevo.Id;
        }
        else
        {
            throw new BusinessException(
                code: "CLIENTE_REQUERIDO",
                message: "Debe indicar un cliente existente o los datos de un cliente nuevo.");
        }

        return await ConstruirYPersistirTurnoAsync(
            tenantId, clienteId, dto.RecursoId, dto.ServicioIds, dto.FechaHoraInicio, cancellationToken);
    }

    public async Task<TurnoDto> CrearTurnoPublicoAsync(int clienteId, CrearTurnoPublicoDto dto, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        // El cliente ya está logueado (y las reservas públicas estaban habilitadas cuando se
        // registró), pero el staff pudo haberlas desactivado desde entonces — re-chequear acá.
        if (!tenant.PermiteReservasPublicas)
            throw new BusinessException(
                code: "RESERVAS_PUBLICAS_DESHABILITADAS",
                message: "Este negocio no tiene reservas online habilitadas en este momento.");

        return await ConstruirYPersistirTurnoAsync(
            tenantId, clienteId, dto.RecursoId, dto.ServicioIds, dto.FechaHoraInicio, cancellationToken);
    }

    // Compartido por CrearTurnoAsync (staff) y CrearTurnoPublicoAsync (self-service): ambos ya
    // resolvieron su ClienteId a su manera (Paso 0 de a mano vs JWT) antes de llegar acá.
    private async Task<TurnoDto> ConstruirYPersistirTurnoAsync(
        int tenantId, int clienteId, int recursoId, IReadOnlyList<int> servicioIds, DateTime fechaHoraInicio, CancellationToken cancellationToken)
    {
        // ── Paso 1: Validar y obtener TODOS los servicios ─────────────────────
        if (servicioIds is null || servicioIds.Count == 0)
            throw new BusinessException(
                code: "SERVICIOS_REQUERIDOS",
                message: "El turno debe incluir al menos un servicio.");

        var servicios = new List<Servicio>();
        foreach (var servicioId in servicioIds)
        {
            var servicio = await _unitOfWork.Servicios.GetByIdAsync(servicioId, cancellationToken);

            if (servicio is null)
                throw new NotFoundException(nameof(Servicio), servicioId);

            servicios.Add(servicio);
        }

        // ── Paso 2: Calcular FechaHoraFin (suma de todas las duraciones) ──────
        var duracionTotalMinutos = servicios.Sum(s => s.DuracionMinutos);
        var fechaHoraFin = fechaHoraInicio.AddMinutes(duracionTotalMinutos);

        // ── Paso 3: Leer PermiteSolapamiento del Tenant ──────────────────────
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        // ── Paso 4: Validar solapamiento ──────────────────────────────────────
        await _solapamientoValidator.ValidarAsync(
            recursoId: recursoId,
            inicio: fechaHoraInicio,
            fin: fechaHoraFin,
            permiteSolapamiento: tenant.PermiteSolapamiento,
            cancellationToken: cancellationToken);

        // ── Paso 5: Construir entidad con TODOS los TurnoServicio ─────────────
        var turno = new Turno
        {
            TenantId = tenantId,
            ClienteId = clienteId,
            RecursoId = recursoId,
            FechaHoraInicio = fechaHoraInicio,
            Estado = EstadoTurno.Pendiente,
            CreadoPor = tenantId.ToString()
        };

        for (var i = 0; i < servicios.Count; i++)
        {
            turno.TurnoServicios.Add(new TurnoServicio
            {
                ServicioId = servicios[i].Id,
                Orden = i + 1,
                PrecioAplicado = servicios[i].Precio
            });
        }

        // ── Paso 6: Persistir ─────────────────────────────────────────────────
        await _unitOfWork.Turnos.AddAsync(turno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Releemos con detalles para armar el DTO completo (nombres, precio total)
        var turnoCompleto = await _unitOfWork.Turnos.GetByIdConDetallesAsync(turno.Id, cancellationToken);

        await _notificacionAppService.ProgramarConfirmacionAsync(turnoCompleto!, cancellationToken);
        await _notificacionAppService.ProgramarRecordatorioAsync(turnoCompleto!, cancellationToken);

        return MapToDto(turnoCompleto!);
    }

    public async Task CancelarTurnoAsync(int id, CancellationToken cancellationToken = default)
    {
        // Con detalles: necesitamos TurnoServicios/Recurso para el rango real del turno y
        // para el matching/aviso de lista de espera hecho a continuación.
        var turno = await _unitOfWork.Turnos.GetByIdConDetallesAsync(id, cancellationToken);

        if (turno is null)
            throw new NotFoundException(nameof(Turno), id);

        if (turno.Estado == EstadoTurno.Cancelado)
            throw new BusinessException(
                code: "TURNO_YA_CANCELADO",
                message: $"El turno {id} ya se encuentra cancelado.");

        await CancelarTurnoInternoAsync(turno, cancellationToken);
    }

    public async Task<IReadOnlyList<TurnoDto>> GetMisTurnosAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        var turnos = await _unitOfWork.Turnos.GetAllConDetallesAsync(cancellationToken);

        return turnos
            .Where(t => t.ClienteId == clienteId)
            .Select(MapToDto)
            .ToList();
    }

    public async Task CancelarPropioAsync(int clienteId, int turnoId, CancellationToken cancellationToken = default)
    {
        var turno = await _unitOfWork.Turnos.GetByIdConDetallesAsync(turnoId, cancellationToken);

        if (turno is null)
            throw new NotFoundException(nameof(Turno), turnoId);

        if (turno.ClienteId != clienteId)
            throw new ForbiddenException("Este turno no te pertenece.");

        if (turno.Estado == EstadoTurno.Cancelado)
            throw new BusinessException(
                code: "TURNO_YA_CANCELADO",
                message: $"El turno {turnoId} ya se encuentra cancelado.");

        await CancelarTurnoInternoAsync(turno, cancellationToken);
    }

    // Compartido por CancelarTurnoAsync (staff) y CancelarPropioAsync (self-service): cada
    // caller valida existencia/estado/ownership a su manera antes de llegar acá.
    private async Task CancelarTurnoInternoAsync(Turno turno, CancellationToken cancellationToken)
    {
        await ValidarNoLiquidadoAsync(turno.Id, cancellationToken);

        turno.Estado = EstadoTurno.Cancelado;
        turno.ModificadoEn = DateTime.UtcNow;

        _unitOfWork.Turnos.Update(turno);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificacionAppService.CancelarPendientesDeTurnoAsync(turno.Id, cancellationToken);
        await NotificarListaEsperaAsync(turno, cancellationToken);
    }

    // El slot ya vuelve a estar disponible solo con la cancelación de arriba (Solapamiento
    // Validator/ExisteTurnoEnRangoAsync ya excluyen turnos Cancelado). Acá solo avisamos a
    // quienes esperaban ese horario.
    private async Task NotificarListaEsperaAsync(Turno turno, CancellationToken cancellationToken)
    {
        var servicioIds = turno.TurnoServicios.Select(ts => ts.ServicioId).ToList();
        var fechaHoraFin = turno.FechaHoraInicio.AddMinutes(
            turno.TurnoServicios.Sum(ts => ts.Servicio?.DuracionMinutos ?? 0));

        var coincidencias = await _unitOfWork.ListasEspera.BuscarCoincidenciasAsync(
            turno.RecursoId, servicioIds, turno.FechaHoraInicio, fechaHoraFin, cancellationToken);

        if (coincidencias.Count == 0)
            return;

        foreach (var entrada in coincidencias)
        {
            entrada.Estado = EstadoListaEspera.Notificada;
            entrada.NotificadoEn = DateTime.UtcNow;
            _unitOfWork.ListasEspera.Update(entrada);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var entrada in coincidencias)
        {
            await _notificacionAppService.ProgramarListaEsperaAsync(entrada, turno, cancellationToken);
        }
    }

    public async Task<TurnoDto> CambiarEstadoTurnoAsync(int id, CambiarEstadoTurnoDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<EstadoTurno>(dto.NuevoEstado, ignoreCase: true, out var nuevoEstado))
            throw new BadRequestException(
                $"'{dto.NuevoEstado}' no es un estado de turno válido.");

        if (nuevoEstado == EstadoTurno.Cancelado)
            throw new BusinessException(
                code: "USAR_CANCELAR",
                message: "Para cancelar un turno usá la acción de cancelar, no el cambio de estado.");

        var turno = await _unitOfWork.Turnos.GetByIdAsync(id, cancellationToken);

        if (turno is null)
            throw new NotFoundException(nameof(Turno), id);

        if (turno.Estado == EstadoTurno.Cancelado)
            throw new BusinessException(
                code: "TURNO_CANCELADO",
                message: $"El turno {id} está cancelado y no se puede cambiar su estado.");

        await ValidarNoLiquidadoAsync(id, cancellationToken);

        turno.Estado = nuevoEstado;
        turno.ModificadoEn = DateTime.UtcNow;

        _unitOfWork.Turnos.Update(turno);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var turnoCompleto = await _unitOfWork.Turnos.GetByIdConDetallesAsync(id, cancellationToken);

        return MapToDto(turnoCompleto!);
    }

    // Un turno con algún LiquidacionDetalle vigente (Liquidacion no Anulada) ya generó
    // comisión — cancelarlo o cambiarle el estado después alteraría un cálculo ya cerrado.
    private async Task ValidarNoLiquidadoAsync(int turnoId, CancellationToken cancellationToken)
    {
        var yaLiquidado = await _unitOfWork.LiquidacionDetalles.ExisteVigentePorTurnoAsync(turnoId, cancellationToken);

        if (yaLiquidado)
            throw new BusinessException(
                code: "TURNO_YA_LIQUIDADO",
                message: $"El turno {turnoId} ya fue liquidado y no puede modificarse.");
    }

    private static TurnoDto MapToDto(Turno turno)
    {
        var precioTotal = turno.TurnoServicios.Sum(ts => ts.PrecioAplicado);
        var montoCobrado = turno.Cobros.Sum(c => c.PrecioBase);
        var saldoPendiente = precioTotal - montoCobrado;

        var estadoPago = montoCobrado <= 0
            ? EstadoPagoTurno.SinCobrar
            : saldoPendiente <= 0
                ? EstadoPagoTurno.Pagado
                : EstadoPagoTurno.Parcial;

        return new TurnoDto(
            Id: turno.Id,
            RecursoId: turno.RecursoId,
            RecursoNombre: turno.Recurso?.Nombre ?? string.Empty,
            ClienteId: turno.ClienteId,
            ClienteNombreCompleto: $"{turno.Cliente?.Nombre} {turno.Cliente?.Apellido}".Trim(),
            FechaHoraInicio: turno.FechaHoraInicio,
            FechaHoraFin: turno.FechaHoraInicio.AddMinutes(
                turno.TurnoServicios.Sum(ts => ts.Servicio?.DuracionMinutos ?? 0)),
            Estado: turno.Estado.ToString(),
            Servicios: turno.TurnoServicios.Select(ts => ts.Servicio?.Nombre ?? string.Empty).ToList(),
            PrecioTotal: precioTotal,
            MontoCobrado: montoCobrado,
            SaldoPendiente: saldoPendiente,
            EstadoPago: estadoPago.ToString()
        );
    }
}