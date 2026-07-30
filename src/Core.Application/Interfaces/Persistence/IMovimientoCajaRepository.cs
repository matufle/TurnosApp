using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface IMovimientoCajaRepository : IRepository<MovimientoCaja>
{
    Task<IReadOnlyList<MovimientoCaja>> GetBySesionIdAsync(int sesionCajaId, CancellationToken cancellationToken = default);

    // Busca el movimiento automático (no-reversa) vigente de un Cobro, solo si su sesión sigue Abierta.
    // Usado por CajaAppService.SincronizarMovimientoDeCobroAsync para reversar-y-reemplazar en un ActualizarCobroAsync.
    Task<MovimientoCaja?> GetMovimientoActivoDeCobroAsync(int cobroId, CancellationToken cancellationToken = default);
}
