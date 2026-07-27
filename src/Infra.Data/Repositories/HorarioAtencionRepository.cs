using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class HorarioAtencionRepository : GenericRepository<HorarioAtencion>, IHorarioAtencionRepository
{
    public HorarioAtencionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<HorarioAtencion>> GetByRecursoIdAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.RecursoId == recursoId)
            .OrderBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HorarioAtencion>> GetByRecursoIdCrossTenantAsync(int tenantId, int recursoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(h => h.TenantId == tenantId && h.RecursoId == recursoId)
            .ToListAsync(cancellationToken);
    }
}
