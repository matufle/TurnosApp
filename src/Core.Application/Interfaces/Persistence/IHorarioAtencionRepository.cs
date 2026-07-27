using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface IHorarioAtencionRepository : IRepository<HorarioAtencion>
{
    /// <summary>Tenant-filtrado normal: UI de staff, request autenticado.</summary>
    Task<IReadOnlyList<HorarioAtencion>> GetByRecursoIdAsync(int recursoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cross-tenant a propósito — mismo motivo que Notificacion/Cliente en las Partes 1-2:
    /// se llama desde el cálculo de disponibilidad público, sin JWT que resolver.
    /// </summary>
    Task<IReadOnlyList<HorarioAtencion>> GetByRecursoIdCrossTenantAsync(int tenantId, int recursoId, CancellationToken cancellationToken = default);
}
