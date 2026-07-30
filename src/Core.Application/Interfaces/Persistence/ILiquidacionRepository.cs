using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface ILiquidacionRepository : IRepository<Liquidacion>
{
    Task<Liquidacion?> GetByIdConDetallesAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Liquidacion>> GetAllConDetallesAsync(CancellationToken cancellationToken = default);

    /// <summary>Liquidaciones de un Recurso puntual — usado por "mis liquidaciones" (self-service).</summary>
    Task<IReadOnlyList<Liquidacion>> GetByRecursoConDetallesAsync(int recursoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// La liquidación Generada (no Pagada) de un Recurso cuyo período cubre <paramref name="fecha"/>,
    /// si existe — usado al cargar un adelanto para asignarlo al vuelo en vez de esperar al próximo ciclo.
    /// </summary>
    Task<Liquidacion?> GetGeneradaQueCubreAsync(int recursoId, DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cross-tenant a propósito: usado por LiquidacionGeneratorService para chequear si ya
    /// existe una liquidación de este Recurso+período antes de generar una nueva (el índice
    /// único en la tabla es la defensa en profundidad final).
    /// </summary>
    Task<Liquidacion?> GetByRecursoYPeriodoCrossTenantAsync(int tenantId, int recursoId, DateTime periodoDesde, DateTime periodoHasta, CancellationToken cancellationToken = default);
}
