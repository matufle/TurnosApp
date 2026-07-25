using Microsoft.AspNetCore.Mvc.Filters;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Presentation.WebAPI.Authorization;

/// <summary>
/// Exige que el usuario autenticado tenga el permiso indicado (resuelto contra la base vía
/// ICurrentUserService, no contra el JWT — ver la nota en ICurrentUserService sobre por qué).
/// Convive con [Authorize] a nivel de clase: éste rechaza con 401 al no autenticado antes de
/// que este filtro corra; este filtro sólo distingue autenticado-pero-sin-permiso (403).
/// Se prefiere este filtro simple por sobre un IAuthorizationPolicyProvider dinámico de ASP.NET,
/// más consistente con el resto del proyecto (Service Layer plano, sin CQRS/mediación extra).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequierePermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly Permiso _permiso;

    public RequierePermisoAttribute(Permiso permiso)
    {
        _permiso = permiso;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            return; // [Authorize] ya rechaza esto con 401 antes de llegar acá

        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        var permisos = await currentUserService.GetCurrentPermisosAsync(context.HttpContext.RequestAborted);

        if (!permisos.HasFlag(_permiso))
            throw new ForbiddenException($"No tenés el permiso '{_permiso}' para realizar esta acción.");
    }
}
