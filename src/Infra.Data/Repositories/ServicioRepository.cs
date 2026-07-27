using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class ServicioRepository : GenericRepository<Servicio>, IServicioRepository
{
    public ServicioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Servicio>> GetActivosCrossTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == tenantId && s.Activo)
            .ToListAsync(cancellationToken);
    }

    public async Task<Servicio?> GetByIdCrossTenantAsync(int tenantId, int servicioId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == servicioId, cancellationToken);
    }
}