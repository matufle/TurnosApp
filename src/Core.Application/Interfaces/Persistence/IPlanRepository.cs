using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio de Plan. Sin Global Query Filter (Plan no hereda de TenantEntity,
/// es catálogo global — mismo criterio que Tenant).
/// </summary>
public interface IPlanRepository : IRepository<Plan>
{
    Task<Plan?> GetActivoAsync(CancellationToken cancellationToken = default);
}
