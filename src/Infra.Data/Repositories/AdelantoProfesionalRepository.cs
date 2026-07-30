using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class AdelantoProfesionalRepository : GenericRepository<AdelantoProfesional>, IAdelantoProfesionalRepository
{
    public AdelantoProfesionalRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AdelantoProfesional>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.RecursoId == recursoId)
            .OrderByDescending(a => a.Fecha)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdelantoProfesional>> GetPendientesCrossTenantAsync(
        int tenantId, DateTime hasta, CancellationToken cancellationToken = default)
    {
        // Cross-tenant a propósito: lo llama el worker de liquidaciones, sin JWT que resolver.
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantId && a.LiquidacionId == null && a.Fecha <= hasta)
            .ToListAsync(cancellationToken);
    }
}
