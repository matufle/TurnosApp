using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Roles;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IRolAppService
{
    Task<RolDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RolDto> CreateAsync(CreateRolDto dto, CancellationToken cancellationToken = default);
    Task<RolDto> UpdateAsync(int id, UpdateRolDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
