using Microsoft.Extensions.Diagnostics.HealthChecks;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Presentation.WebAPI.HealthChecks;

public class DbConnectivityHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DbConnectivityHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await _context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.");
    }
}
