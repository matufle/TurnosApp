using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Servicios;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Services;

public class ServicioService : IServicioService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServicioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServicioDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(id, cancellationToken);

        if (servicio is null)
            throw new NotFoundException(nameof(Servicio), id);

        return MapToDto(servicio);
    }

    public async Task<IReadOnlyList<ServicioDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var servicios = await _unitOfWork.Servicios.GetAllAsync(cancellationToken);

        return servicios.Select(MapToDto).ToList();
    }

    public async Task<ServicioDto> CreateAsync(CreateServicioDto dto, CancellationToken cancellationToken = default)
    {
        // El TenantId lo asigna el repositorio / DbContext a través del ITenantProvider.
        // Application no conoce el HttpContext — esa responsabilidad es de Infra/WebAPI.
        var servicio = new Servicio
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            DuracionMinutos = dto.DuracionMinutos,
            Precio = dto.Precio,
            Activo = true
        };

        await _unitOfWork.Servicios.AddAsync(servicio, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(servicio);
    }

    public async Task<ServicioDto> UpdateAsync(int id, UpdateServicioDto dto, CancellationToken cancellationToken = default)
    {
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(id, cancellationToken);

        if (servicio is null)
            throw new NotFoundException(nameof(Servicio), id);

        // Mapeo explícito de campos editables
        servicio.Nombre = dto.Nombre;
        servicio.Descripcion = dto.Descripcion;
        servicio.DuracionMinutos = dto.DuracionMinutos;
        servicio.Precio = dto.Precio;
        servicio.Activo = dto.Activo;

        _unitOfWork.Servicios.Update(servicio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(servicio);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(id, cancellationToken);

        if (servicio is null)
            throw new NotFoundException(nameof(Servicio), id);

        _unitOfWork.Servicios.Delete(servicio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Mapeo privado centralizado — un solo lugar para cambiar si el DTO evoluciona.
    // -------------------------------------------------------------------------

    private static ServicioDto MapToDto(Servicio servicio) => new(
        Id: servicio.Id,
        Nombre: servicio.Nombre,
        Descripcion: servicio.Descripcion,
        DuracionMinutos: servicio.DuracionMinutos,
        Precio: servicio.Precio,
        Activo: servicio.Activo
    );
}
