using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;
using TurnosApp.Infra.Data.Repositories;

namespace TurnosApp.Infra.Data.Repositories
{
    public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
    {
        public TenantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        }
    }
}
