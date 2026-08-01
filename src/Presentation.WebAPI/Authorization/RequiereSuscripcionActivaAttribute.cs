using Microsoft.AspNetCore.Mvc.Filters;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Presentation.WebAPI.Authorization;

/// <summary>
/// Filtro global (registrado en Program.cs vía AddControllers(options => options.Filters.Add
/// &lt;RequiereSuscripcionActivaAttribute&gt;()), no atributo por-controller como RequierePermiso)
/// que bloquea a un tenant cuya suscripción no está activa ni en trial vigente. Grandfathering
/// total: todo tenant con EsGrandfathered=true (todos los que existían antes de este deploy,
/// ver migración AgregarSuscripcionMercadoPago) nunca pasa por este chequeo — decisión de producto
/// confirmada con Mateo para no trabar a nadie que ya estaba usando la app.
/// Rutas exentas por prefijo (mismo patrón que TenantMiddleware.IsExemptPath): /api/auth (para
/// que /me siga funcionando y la UI pueda mostrar el aviso de suscripción vencida) y
/// /api/suscripciones (para que el tenant bloqueado pueda igual pagar/gestionar su suscripción).
/// Como es un filtro global corre en TODAS las acciones, incluidas las de Cliente (JWT propio,
/// misma claim TenantId) — un tenant sin pagar también bloquea la reserva pública, por diseño.
/// </summary>
public class RequiereSuscripcionActivaAttribute : Attribute, IAsyncAuthorizationFilter
{
    private static readonly string[] RutasExentas =
    [
        "/api/auth",
        "/api/suscripciones",
    ];

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            return; // anónimo: [Authorize] (o su ausencia) ya decide esto antes de llegar acá

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        if (RutasExentas.Any(ruta => path.StartsWith(ruta, StringComparison.OrdinalIgnoreCase)))
            return;

        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

        var tenantId = currentUserService.GetCurrentTenantId();
        var tenant = await unitOfWork.Tenants.GetByIdAsync(tenantId, context.HttpContext.RequestAborted);

        if (tenant is null || tenant.EsGrandfathered)
            return;

        var suscripcionActiva = tenant.EstadoSuscripcion == EstadoSuscripcion.Activa
            || (tenant.EstadoSuscripcion == EstadoSuscripcion.Trial && tenant.SuscripcionVenceEn > DateTime.UtcNow);

        if (!suscripcionActiva)
        {
            throw new BusinessException(
                code: "SUSCRIPCION_INACTIVA",
                message: "Tu suscripción no está activa. Actualizá tu método de pago para seguir usando Turnify.");
        }
    }
}
