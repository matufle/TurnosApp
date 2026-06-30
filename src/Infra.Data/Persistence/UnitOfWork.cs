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

    // Repositorios recibidos por DI — misma instancia Scoped que el DbContext.
    public ITenantRepository Tenants { get; }
    public IServicioRepository Servicios { get; }
    public IClienteRepository Clientes { get; }
    public ITurnoRepository Turnos { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        ITenantRepository tenants,
        IServicioRepository servicios,
        IClienteRepository clientes,
        ITurnoRepository turnos)
    {
        _context = context;
        Tenants = tenants;
        Servicios = servicios;
        Clientes = clientes;
        Turnos = turnos;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _context.Dispose();
}