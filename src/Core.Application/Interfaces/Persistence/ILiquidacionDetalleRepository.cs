using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface ILiquidacionDetalleRepository : IRepository<LiquidacionDetalle>
{
    /// <summary>
    /// True si el turno tiene algún detalle de liquidación cuya Liquidacion no esté Anulada —
    /// usado por TurnoAppService para bloquear cancelación/cambio de estado de turnos ya liquidados.
    /// </summary>
    Task<bool> ExisteVigentePorTurnoAsync(int turnoId, CancellationToken cancellationToken = default);
}
