using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Tenants;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class TenantAppService : ITenantAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public TenantAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _unitOfWork.Tenants.GetAllAsync(cancellationToken);
        return tenants.Select(MapToDto).ToList();
    }

    public async Task<TenantDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), id);

        return MapToDto(tenant);
    }

    public async Task<TenantDto> CreateAsync(CreateTenantDto dto, CancellationToken cancellationToken = default)
    {
        var tenant = new Tenant
        {
            Nombre = dto.Nombre,
            Slug = dto.Slug,
            PermitirSolapamiento = dto.PermitirSolapamiento,
            Activo = true,
            FechaAlta = DateTime.UtcNow
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(tenant);
    }

    private static TenantDto MapToDto(Tenant t) => new(
        Id: t.Id,
        Nombre: t.Nombre,
        Slug: t.Slug,
        PermitirSolapamiento: t.PermitirSolapamiento,
        Activo: t.Activo,
        FechaAlta: t.FechaAlta
    );
}