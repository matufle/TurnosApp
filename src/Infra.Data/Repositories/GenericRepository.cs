using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Core.Domain.Common;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // FindAsync respeta el Global Query Filter en EF Core 8
        // y aprovecha el cache del change tracker antes de ir a la DB.
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Si la entidad pertenece a un tenant, asignamos el TenantId aquí —
        // Application no debe conocer este detalle de infraestructura.
        if (entity is TenantEntity tenantEntity)
        {
            // ITenantProvider está disponible vía el DbContext que ya lo recibe.
            tenantEntity.TenantId = _context.CurrentTenantId;
        }

        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        // Attach + mark Modified cubre el caso en que la entidad
        // fue recuperada con AsNoTracking() o viene de una capa externa.
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
}