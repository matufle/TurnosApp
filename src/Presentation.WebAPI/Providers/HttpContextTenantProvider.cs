using TurnosApp.Core.Application.Interfaces;

namespace TurnosApp.Presentation.WebAPI.Providers;

/// <summary>
/// Resuelve el TenantId actual leyendo el header HTTP "X-Tenant-Id".
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

        var headerValue = context.Request.Headers[HeaderName].FirstOrDefault();

        // El TenantMiddleware ya validó que el header existe y es un int válido.
        // Si llegamos aquí sin middleware (tests, etc.), fallamos con mensaje claro.
        if (!int.TryParse(headerValue, out var tenantId) || tenantId <= 0)
            throw new InvalidOperationException(
                $"El header '{HeaderName}' no contiene un TenantId válido. " +
                $"Valor recibido: '{headerValue}'.");

        return tenantId;
    }
}