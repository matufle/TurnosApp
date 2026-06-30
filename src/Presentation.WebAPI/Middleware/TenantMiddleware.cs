using TurnosApp.Presentation.WebAPI.Providers;

namespace TurnosApp.Presentation.WebAPI.Middleware;

/// <summary>
/// Valida la presencia y formato del header X-Tenant-Id en cada request.
/// Debe registrarse antes de UseRouting() y UseAuthorization() para
/// cortar el pipeline temprano si el header es inválido.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Excluimos Swagger y health checks del requisito de tenant.
        if (IsExemptPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var headerValue = context.Request.Headers[HttpContextTenantProvider.HeaderName]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(headerValue))
        {
            _logger.LogWarning(
                "Request a {Path} sin header {Header}",
                context.Request.Path,
                HttpContextTenantProvider.HeaderName);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = $"El header '{HttpContextTenantProvider.HeaderName}' es obligatorio."
            });
            return;
        }

        if (!int.TryParse(headerValue, out var tenantId) || tenantId <= 0)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = $"El header '{HttpContextTenantProvider.HeaderName}' " +
                        $"debe ser un número entero positivo. Valor: '{headerValue}'."
            });
            return;
        }

        await _next(context);
    }

    private static bool IsExemptPath(PathString path) =>
        path.StartsWithSegments("/swagger") ||
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments("/favicon.ico");
}
