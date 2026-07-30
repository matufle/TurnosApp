using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface ISesionCajaRepository : IRepository<SesionCaja>
{
    Task<SesionCaja?> GetAbiertaAsync(CancellationToken cancellationToken = default);

    Task<SesionCaja?> GetByIdConMovimientosAsync(int id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<SesionCaja> Items, int TotalCount)> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default);
}
