using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Auth;
using TurnosApp.Core.Application.DTOs.Usuarios;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IUsuarioAppService
{
    Task<UsuarioDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioDto> ActivarAsync(int id, CancellationToken cancellationToken = default);
    Task<UsuarioDto> DesactivarAsync(int id, CancellationToken cancellationToken = default);
    Task<MeDto> GetMeAsync(CancellationToken cancellationToken = default);
}
