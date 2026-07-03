using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Tenants;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ITenantAppService
{
    Task<IReadOnlyList<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TenantDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TenantDto> CreateAsync(CreateTenantDto dto, CancellationToken cancellationToken = default);
}