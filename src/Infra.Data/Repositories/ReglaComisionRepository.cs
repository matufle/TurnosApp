using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class ReglaComisionRepository : GenericRepository<ReglaComision>, IReglaComisionRepository
{
    public ReglaComisionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ReglaComision>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Servicio)
            .Where(r => r.RecursoId == recursoId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReglaComision>> GetVigentesCrossTenantAsync(
        int tenantId, int recursoId, CancellationToken cancellationToken = default)
    {
        // Cross-tenant a propósito: lo llama el worker de liquidaciones, sin JWT que resolver.
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && r.RecursoId == recursoId && r.Activo)
            .ToListAsync(cancellationToken);
    }
}
