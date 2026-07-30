using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Presentation.WebAPI.Authorization;

/// <summary>
/// Exige que el usuario autenticado tenga AL MENOS UNO de los permisos indicados (resuelto
/// contra la base vía ICurrentUserService, no contra el JWT — ver la nota en ICurrentUserService
/// sobre por qué). Con un solo permiso es una exigencia simple; con varios es un OR — ej.
/// [RequierePermiso(Permiso.VerCaja, Permiso.GestionarCaja)] deja pasar a quien pueda operar
/// la caja aunque no tenga el permiso de solo-lectura por separado.
/// Convive con [Authorize] a nivel de clase: éste rechaza con 401 al no autenticado antes de
/// que este filtro corra; este filtro sólo distingue autenticado-pero-sin-permiso (403).
/// Se prefiere este filtro simple por sobre un IAuthorizationPolicyProvider dinámico de ASP.NET,
/// más consistente con el resto del proyecto (Service Layer plano, sin CQRS/mediación extra).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequierePermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly Permiso[] _permisos;

    public RequierePermisoAttribute(params Permiso[] permisos)
    {
        _permisos = permisos;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            return; // [Authorize] ya rechaza esto con 401 antes de llegar acá

        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        var permisos = await currentUserService.GetCurrentPermisosAsync(context.HttpContext.RequestAborted);

        if (!_permisos.Any(p => permisos.HasFlag(p)))
            throw new ForbiddenException($"No tenés ninguno de los permisos requeridos ({string.Join(", ", _permisos)}) para realizar esta acción.");
    }
}
