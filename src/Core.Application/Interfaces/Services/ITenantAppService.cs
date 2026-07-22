using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Tenant;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ITenantAppService
{
    Task<TenantConfigDto> GetConfigAsync(CancellationToken cancellationToken = default);

    Task UpdateConfigAsync(UpdateTenantConfigDto dto, CancellationToken cancellationToken = default);
}