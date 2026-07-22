using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Tenant;
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

    public async Task<TenantConfigDto> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        // 1. Obtenemos el ID de forma dinámica sin tocar el protocolo HTTP
        var tenantId = _currentUserService.GetCurrentTenantId();

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        return new TenantConfigDto(
            Nombre: tenant.Nombre,
            PermiteSolapamiento: tenant.PermiteSolapamiento,
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
        tenant.PermiteSolapamiento = dto.PermiteSolapamiento;
        // 3. Como EF Core trackea la entidad, solo guardamos
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}