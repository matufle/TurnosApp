using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Providers;

/// <summary>
/// Resuelve el TenantId actual a partir del claim "TenantId" del JWT autenticado.
/// El header HTTP "X-Tenant-Id" (validado por TenantMiddleware) sólo se usa como
/// chequeo de forma en la request; NUNCA como fuente de autorización, porque es
/// un valor que controla el cliente y podría manipularse para acceder a datos
/// de otro tenant (IDOR). El TenantId real sale del token firmado por el server.
/// Se registra como Scoped: una instancia por request, igual que el DbContext
/// que la consume — garantiza consistencia dentro del mismo ciclo de vida.
/// </summary>
public class HttpContextTenantProvider : ITenantProvider
{
    public const string HeaderName = "X-Tenant-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentTenantId()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context is null)
            throw new InvalidOperationException(
                "HttpContext no disponible. ITenantProvider solo puede " +
                "usarse dentro del contexto de una request HTTP.");

        var tenantClaim = context.User?.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantClaim) || !int.TryParse(tenantClaim, out var tenantId) || tenantId <= 0)
            throw new InvalidOperationException(
                "No se pudo identificar el TenantId a partir del token autenticado.");

        return tenantId;
    }
}