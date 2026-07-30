using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface IReglaComisionRepository : IRepository<ReglaComision>
{
    Task<IReadOnlyList<ReglaComision>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cross-tenant a propósito: usado por LiquidacionGeneratorService (worker en background,
    /// sin JWT que resolver). Trae todas las reglas Activo de un Recurso (base + overrides por
    /// servicio) para que el caller resuelva la prioridad servicio &gt; base en memoria.
    /// </summary>
    Task<IReadOnlyList<ReglaComision>> GetVigentesCrossTenantAsync(int tenantId, int recursoId, CancellationToken cancellationToken = default);
}
