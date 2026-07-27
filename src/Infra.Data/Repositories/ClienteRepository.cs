using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Cliente?> GetByTenantYEmailAsync(int tenantId, string emailNormalizado, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c =>
                c.TenantId == tenantId &&
                c.Email != null &&
                c.Email.ToLower() == emailNormalizado,
                cancellationToken);
    }
}