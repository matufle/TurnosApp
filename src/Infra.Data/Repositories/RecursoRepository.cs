using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class RecursoRepository : GenericRepository<Recurso>, IRecursoRepository
{
    public RecursoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Recurso?> GetByUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await _context.Recursos
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId, cancellationToken);
}
