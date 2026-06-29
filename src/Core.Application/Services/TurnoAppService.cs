using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Turnos;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;

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

    public async Task<TurnoDto> CrearTurnoAsync(
        CrearTurnoDto dto,
        CancellationToken cancellationToken = default)
    {
        // ── Paso 1: Obtener el Servicio para conocer la duración ──────────────
        var servicio = await _unitOfWork.Servicios
            .GetByIdAsync(dto.ServicioId, cancellationToken);

        if (servicio is null)
            throw new NotFoundException(nameof(Servicio), dto.ServicioId);

        // ── Paso 2: Calcular FechaHoraFin ─────────────────────────────────────
        var fechaHoraFin = dto.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);

        // ── Paso 3: Obtener el Tenant para leer PermitirSolapamiento ──────────
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var tenant = await _unitOfWork.Tenants
            .GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        // ── Paso 4: Validar solapamiento ──────────────────────────────────────
        await _solapamientoValidator.ValidarAsync(
            recursoId: dto.RecursoId,
            inicio: dto.FechaHoraInicio,
            fin: fechaHoraFin,
            permitirSolapamiento: tenant.PermitirSolapamiento,
            cancellationToken: cancellationToken);

        // ── Paso 5: Construir la entidad Turno ────────────────────────────────
        var turno = new Turno
        {
            TenantId = tenantId,
            ClienteId = dto.ClienteId,
            RecursoId = dto.RecursoId,
            FechaHoraInicio = dto.FechaHoraInicio,
            Estado = EstadoTurno.Pendiente,
            CreadoPor = tenantId.ToString()   // reemplazar por UserId del JWT cuando haya auth
        };

        // ── Paso 6: Agregar el servicio al turno (tabla pivot TurnoServicio) ──
        turno.TurnoServicios.Add(new TurnoServicio
        {
            ServicioId = dto.ServicioId,
            Orden = 1,
            PrecioAplicado = servicio.Precio    // snapshot del precio actual
        });

        // ── Paso 7: Persistir ─────────────────────────────────────────────────
        // GenericRepository.AddAsync ya no asigna TenantId aquí porque
        // lo asignamos explícitamente arriba (necesitábamos el valor antes).
        await _unitOfWork.Turnos.AddAsync(turno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 8: Retornar el DTO con los datos calculados ──────────────────
        return new TurnoDto(
            Id: turno.Id,
            ClienteId: turno.ClienteId,
            RecursoId: turno.RecursoId,
            ServicioId: dto.ServicioId,
            FechaHoraInicio: turno.FechaHoraInicio,
            FechaHoraFin: fechaHoraFin,
            Estado: turno.Estado
        );
    }
}
