using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class SesionCajaRepository : GenericRepository<SesionCaja>, ISesionCajaRepository
{
    public SesionCajaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SesionCaja?> GetAbiertaAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Movimientos)
            .FirstOrDefaultAsync(s => s.Estado == EstadoSesionCaja.Abierta, cancellationToken);
    }

    public async Task<SesionCaja?> GetByIdConMovimientosAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Movimientos)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<SesionCaja> Items, int TotalCount)> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(s => s.Movimientos)
            .Where(s => s.Estado == EstadoSesionCaja.Cerrada)
            .AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(s => s.FechaApertura >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(s => s.FechaApertura <= fechaHasta.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.FechaApertura)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
