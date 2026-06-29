using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Ejemplos de extensiones futuras:
    // public async Task<Cliente?> GetByEmailAsync(string email, CancellationToken ct)
    //     => await _dbSet.FirstOrDefaultAsync(c => c.Email == email, ct);
}