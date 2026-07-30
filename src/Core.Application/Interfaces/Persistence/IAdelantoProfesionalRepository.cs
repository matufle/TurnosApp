using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface IAdelantoProfesionalRepository : IRepository<AdelantoProfesional>
{
    Task<IReadOnlyList<AdelantoProfesional>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cross-tenant a propósito: usado por LiquidacionGeneratorService. Adelantos sin asignar
    /// (LiquidacionId null) con fecha hasta el cierre del período que se está generando.
    /// </summary>
    Task<IReadOnlyList<AdelantoProfesional>> GetPendientesCrossTenantAsync(int tenantId, DateTime hasta, CancellationToken cancellationToken = default);
}
