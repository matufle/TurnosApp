using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Infra.Data.Context;
using TurnosApp.Infra.Data.Repositories;

namespace TurnosApp.Infra.Data.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Campos de respaldo para lazy initialization.
    private IServicioRepository? _servicios;
    private IClienteRepository? _clientes;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IServicioRepository Servicios
        => _servicios ??= new ServicioRepository(_context);

    public IClienteRepository Clientes
        => _clientes ??= new ClienteRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _context.Dispose();
}