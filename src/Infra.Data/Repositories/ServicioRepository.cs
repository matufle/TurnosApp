using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class ServicioRepository : GenericRepository<Servicio>, IServicioRepository
{
    public ServicioRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Aquí irán métodos específicos de Servicio cuando los necesitemos.
    // Ejemplo futuro:
    // public async Task<IReadOnlyList<Servicio>> GetActivosAsync(CancellationToken ct)
    //     => await _dbSet.Where(s => s.Activo).AsNoTracking().ToListAsync(ct);
}