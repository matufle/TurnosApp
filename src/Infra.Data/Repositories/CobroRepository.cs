using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class CobroRepository : GenericRepository<Cobro>, ICobroRepository
{
    public CobroRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Cobro>> GetAllByTurnoIdAsync(int turnoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.TurnoId == turnoId)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Cobro> Items, int TotalCount, decimal SumaPrecioFinal, decimal SumaComision)> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? busqueda,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(c => c.Turno)
                .ThenInclude(t => t.Cliente)
            .Include(c => c.Turno)
                .ThenInclude(t => t.TurnoServicios)
                    .ThenInclude(ts => ts.Servicio)
            .AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(c => c.CreadoEn >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(c => c.CreadoEn <= fechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            var patron = $"%{texto}%";

            query = query.Where(c =>
                EF.Functions.ILike(c.Turno.Cliente.Nombre, patron) ||
                EF.Functions.ILike(c.Turno.Cliente.Apellido, patron) ||
                c.TurnoId.ToString() == texto);
        }

        // Cobro.PrecioFinal/MontoComision son propiedades [NotMapped] (aritmética en C#),
        // no traducibles a SQL: materializamos y sumamos/paginamos en memoria.
        var todos = await query
            .OrderByDescending(c => c.CreadoEn)
            .ToListAsync(cancellationToken);

        var totalCount = todos.Count;
        var sumaPrecioFinal = todos.Sum(c => c.PrecioFinal);
        var sumaComision = todos.Sum(c => c.MontoComision);

        var items = todos
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        return (items, totalCount, sumaPrecioFinal, sumaComision);
    }
}
