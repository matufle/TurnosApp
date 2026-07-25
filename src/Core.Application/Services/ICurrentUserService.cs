using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Application.Interfaces.Services;

//Esto sirve para saber quien esta logueado
public interface ICurrentUserService
{
    int GetCurrentTenantId();
    int GetCurrentUsuarioId();

    // Resueltos contra la base (no embebidos en el JWT): un cambio de rol o una
    // desactivación por el Admin aplica de inmediato, sin esperar a que expire el token.
    Task<Permiso> GetCurrentPermisosAsync(CancellationToken cancellationToken = default);
    Task<int?> GetCurrentRecursoIdAsync(CancellationToken cancellationToken = default);
}
