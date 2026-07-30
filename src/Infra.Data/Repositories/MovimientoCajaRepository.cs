using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class MovimientoCajaRepository : GenericRepository<MovimientoCaja>, IMovimientoCajaRepository
{
    public MovimientoCajaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<MovimientoCaja>> GetBySesionIdAsync(int sesionCajaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.SesionCajaId == sesionCajaId)
            .OrderBy(m => m.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<MovimientoCaja?> GetMovimientoActivoDeCobroAsync(int cobroId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.SesionCaja)
            .Where(m =>
                m.CobroId == cobroId &&
                m.MovimientoOrigenId == null &&
                m.SesionCaja.Estado == EstadoSesionCaja.Abierta)
            .OrderByDescending(m => m.FechaHora)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
