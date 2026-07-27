using Microsoft.Extensions.Logging;
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
    private readonly IEmailService _emailService;
    private readonly ILogger<TurnoAppService> _logger;

    public TurnoAppService(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        SolapamientoValidator solapamientoValidator,
        IEmailService emailService,
        ILogger<TurnoAppService> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _solapamientoValidator = solapamientoValidator;
        _emailService = emailService;
        _logger = logger;
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

        // ── Paso 0: Resolver cliente (existente o inline) ─────────────────────
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

        // ── Paso 1: Validar y obtener TODOS los servicios ─────────────────────
        if (dto.ServicioIds is null || dto.ServicioIds.Count == 0)
            throw new BusinessException(
                code: "SERVICIOS_REQUERIDOS",
                message: "El turno debe incluir al menos un servicio.");

        var servicios = new List<Servicio>();
        foreach (var servicioId in dto.ServicioIds)
        {
            var servicio = await _unitOfWork.Servicios.GetByIdAsync(servicioId, cancellationToken);

            if (servicio is null)
                throw new NotFoundException(nameof(Servicio), servicioId);

            servicios.Add(servicio);
        }

        // ── Paso 2: Calcular FechaHoraFin (suma de todas las duraciones) ──────
        var duracionTotalMinutos = servicios.Sum(s => s.DuracionMinutos);
        var fechaHoraFin = dto.FechaHoraInicio.AddMinutes(duracionTotalMinutos);

        // ── Paso 3: Leer PermiteSolapamiento del Tenant ──────────────────────
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        // ── Paso 4: Validar solapamiento ──────────────────────────────────────
        await _solapamientoValidator.ValidarAsync(
            recursoId: dto.RecursoId,
            inicio: dto.FechaHoraInicio,
            fin: fechaHoraFin,
            permiteSolapamiento: tenant.PermiteSolapamiento,
            cancellationToken: cancellationToken);

        // ── Paso 5: Construir entidad con TODOS los TurnoServicio ─────────────
        var turno = new Turno
        {
            TenantId = tenantId,
            ClienteId = clienteId,
            RecursoId = dto.RecursoId,
            FechaHoraInicio = dto.FechaHoraInicio,
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

        turno.Estado = EstadoTurno.Cancelado;
        turno.ModificadoEn = DateTime.UtcNow;

        _unitOfWork.Turnos.Update(turno);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        var recursoNombre = turno.Recurso?.Nombre ?? "el recurso solicitado";

        foreach (var entrada in coincidencias)
        {
            entrada.Estado = EstadoListaEspera.Notificada;
            entrada.NotificadoEn = DateTime.UtcNow;
            _unitOfWork.ListasEspera.Update(entrada);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var entrada in coincidencias)
        {
            if (string.IsNullOrWhiteSpace(entrada.Cliente?.Email))
                continue;

            try
            {
                await _emailService.EnviarAsync(
                    entrada.Cliente.Email,
                    "¡Se liberó un turno que estabas esperando!",
                    $"Hola {entrada.Cliente.Nombre}, se liberó un horario con {recursoNombre} " +
                    $"el {turno.FechaHoraInicio:dd/MM/yyyy HH:mm}. Contactanos para reservarlo antes de que se ocupe.",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Un fallo de SMTP no debe revertir ni afectar la cancelación del turno, ya persistida.
                _logger.LogError(ex, "Fallo al notificar a la lista de espera {ListaEsperaId}", entrada.Id);
            }
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

        turno.Estado = nuevoEstado;
        turno.ModificadoEn = DateTime.UtcNow;

        _unitOfWork.Turnos.Update(turno);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var turnoCompleto = await _unitOfWork.Turnos.GetByIdConDetallesAsync(id, cancellationToken);

        return MapToDto(turnoCompleto!);
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