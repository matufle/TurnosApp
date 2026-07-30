using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class LiquidacionRepository : GenericRepository<Liquidacion>, ILiquidacionRepository
{
    public LiquidacionRepository(ApplicationDbContext context) : base(context)
    {
    }

    private IQueryable<Liquidacion> ConDetalles() =>
        _dbSet
            .Include(l => l.Recurso)
            .Include(l => l.UsuarioPago)
            .Include(l => l.Detalles)
                .ThenInclude(d => d.Turno)
            .Include(l => l.Detalles)
                .ThenInclude(d => d.Servicio)
            .Include(l => l.Adelantos);

    public async Task<Liquidacion?> GetByIdConDetallesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ConDetalles().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Liquidacion>> GetAllConDetallesAsync(CancellationToken cancellationToken = default)
    {
        return await ConDetalles()
            .AsNoTracking()
            .OrderByDescending(l => l.PeriodoDesde)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Liquidacion>> GetByRecursoConDetallesAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        return await ConDetalles()
            .AsNoTracking()
            .Where(l => l.RecursoId == recursoId)
            .OrderByDescending(l => l.PeriodoDesde)
            .ToListAsync(cancellationToken);
    }

    public async Task<Liquidacion?> GetGeneradaQueCubreAsync(int recursoId, DateTime fecha, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Adelantos)
            .FirstOrDefaultAsync(l =>
                l.RecursoId == recursoId &&
                l.Estado == EstadoLiquidacion.Generada &&
                l.PeriodoDesde <= fecha && fecha <= l.PeriodoHasta,
                cancellationToken);
    }

    public async Task<Liquidacion?> GetByRecursoYPeriodoCrossTenantAsync(
        int tenantId, int recursoId, DateTime periodoDesde, DateTime periodoHasta, CancellationToken cancellationToken = default)
    {
        // Cross-tenant a propósito: lo llama el worker de liquidaciones, sin JWT que resolver.
        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l =>
                l.TenantId == tenantId &&
                l.RecursoId == recursoId &&
                l.PeriodoDesde == periodoDesde &&
                l.PeriodoHasta == periodoHasta,
                cancellationToken);
    }
}
