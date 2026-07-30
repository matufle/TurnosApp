using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class LiquidacionDetalleRepository : GenericRepository<LiquidacionDetalle>, ILiquidacionDetalleRepository
{
    public LiquidacionDetalleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExisteVigentePorTurnoAsync(int turnoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Liquidacion)
            .AnyAsync(d => d.TurnoId == turnoId && d.Liquidacion.Estado != EstadoLiquidacion.Anulada, cancellationToken);
    }
}
