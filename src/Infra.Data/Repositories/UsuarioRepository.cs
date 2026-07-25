using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters es intencional: el login ocurre ANTES de conocer el tenant
        return await _context.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim(), cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken)
        => await _context.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<int> ContarPorRolAsync(int rolId, CancellationToken cancellationToken)
        => await _context.Usuarios.CountAsync(u => u.RolId == rolId, cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken)
        => await _context.Usuarios.AddAsync(usuario, cancellationToken);
}
