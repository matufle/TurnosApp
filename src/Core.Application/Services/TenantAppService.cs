using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Tenant;
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
    private readonly ICurrentUserService _currentUserService;

    public TenantAppService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
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
            FechaAlta = DateTime.UtcNow,
            ColorPrimario = dto.ColorPrimario,
            PermiteReservasPublicas = dto.PermiteReservasPublicas
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(tenant);
    }

    public async Task<TenantConfigDto> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        // 1. Obtenemos el ID de forma dinámica sin tocar el protocolo HTTP
        var tenantId = _currentUserService.GetCurrentTenantId();

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        return new TenantConfigDto(
            Nombre: tenant.Nombre,
            ColorPrimario: tenant.ColorPrimario,
            PermiteReservasPublicas: tenant.PermiteReservasPublicas
        );
    }

    public async Task UpdateConfigAsync(UpdateTenantConfigDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Obtenemos el ID dinámico
        var tenantId = _currentUserService.GetCurrentTenantId();

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        // 2. Pisamos los valores
        tenant.ColorPrimario = dto.ColorPrimario;
        tenant.PermiteReservasPublicas = dto.PermiteReservasPublicas;

        // 3. Como EF Core trackea la entidad, solo guardamos
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }

    private static TenantDto MapToDto(Tenant t) => new(
        Id: t.Id,
        Nombre: t.Nombre,
        Slug: t.Slug,
        PermitirSolapamiento: t.PermitirSolapamiento,
        Activo: t.Activo,
        FechaAlta: t.FechaAlta,
        PermiteReservasPublicas: t.PermiteReservasPublicas,
        ColorPrimario: t.ColorPrimario
    );
}