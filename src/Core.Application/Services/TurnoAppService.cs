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
    private readonly SolapamientoValidator _solapamientoValidator;

    public TurnoAppService(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        SolapamientoValidator solapamientoValidator)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _solapamientoValidator = solapamientoValidator;
    }

    public async Task<IReadOnlyList<TurnoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var turnos = await _unitOfWork.Turnos.GetAllAsync(cancellationToken);

        return turnos
            .Where(t => t.Estado != EstadoTurno.Cancelado)
            .Select(t => MapToDto(t))
            .ToList();
    }

    public async Task<TurnoDto> CrearTurnoAsync(CrearTurnoDto dto, CancellationToken cancellationToken = default)
    {
        // ── Paso 1: Obtener servicio para calcular duración ───────────────────
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(dto.ServicioId, cancellationToken);

        if (servicio is null)
            throw new NotFoundException(nameof(Servicio), dto.ServicioId);

        // ── Paso 2: Calcular FechaHoraFin ─────────────────────────────────────
        var fechaHoraFin = dto.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);

        // ── Paso 3: Leer PermitirSolapamiento del Tenant ──────────────────────
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        // ── Paso 4: Validar solapamiento ──────────────────────────────────────
        await _solapamientoValidator.ValidarAsync(
            recursoId: dto.RecursoId,
            inicio: dto.FechaHoraInicio,
            fin: fechaHoraFin,
            permitirSolapamiento: tenant.PermitirSolapamiento,
            cancellationToken: cancellationToken);

        // ── Paso 5: Construir entidad ─────────────────────────────────────────
        var turno = new Turno
        {
            TenantId = tenantId,
            ClienteId = dto.ClienteId,
            RecursoId = dto.RecursoId,
            FechaHoraInicio = dto.FechaHoraInicio,
            Estado = EstadoTurno.Pendiente,
            CreadoPor = tenantId.ToString()
        };

        turno.TurnoServicios.Add(new TurnoServicio
        {
            ServicioId = dto.ServicioId,
            Orden = 1,
            PrecioAplicado = servicio.Precio
        });

        // ── Paso 6: Persistir ─────────────────────────────────────────────────
        await _unitOfWork.Turnos.AddAsync(turno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(turno, dto.ServicioId, fechaHoraFin);
    }

    public async Task CancelarTurnoAsync(int id, CancellationToken cancellationToken = default)
    {
        var turno = await _unitOfWork.Turnos.GetByIdAsync(id, cancellationToken);

        if (turno is null)
            throw new NotFoundException(nameof(Turno), id);

        if (turno.Estado == EstadoTurno.Cancelado)
            throw new BusinessException(
                code: "TURNO_YA_CANCELADO",
                message: $"El turno {id} ya se encuentra cancelado.");

        // Soft Delete: cambia el estado en lugar de eliminar el registro.
        // El historial del turno queda intacto en la base de datos.
        turno.Estado = EstadoTurno.Cancelado;
        turno.ModificadoEn = DateTime.UtcNow;

        _unitOfWork.Turnos.Update(turno);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static TurnoDto MapToDto(Turno turno, int servicioId = 0, DateTime fechaHoraFin = default) => new(
        Id: turno.Id,
        ClienteId: turno.ClienteId,
        RecursoId: turno.RecursoId,
        ServicioId: servicioId > 0
                             ? servicioId
                             : turno.TurnoServicios.FirstOrDefault()?.ServicioId ?? 0,
        FechaHoraInicio: turno.FechaHoraInicio,
        FechaHoraFin: fechaHoraFin != default
                             ? fechaHoraFin
                             : turno.FechaHoraInicio,   // fallback para GET sin cálculo
        Estado: turno.Estado
    );
}