using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class ListaEsperaRepository : GenericRepository<ListaEspera>, IListaEsperaRepository
{
    public ListaEsperaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ListaEspera>> BuscarCoincidenciasAsync(
        int recursoId,
        IReadOnlyCollection<int> servicioIds,
        DateTime inicio,
        DateTime fin,
        CancellationToken cancellationToken = default)
    {
        // Sin AsNoTracking: el caller marca estas entradas como Notificada tras encontrarlas.
        return await _dbSet
            .Include(l => l.Cliente)
            .Where(l =>
                l.RecursoId == recursoId &&
                l.Estado == EstadoListaEspera.Activa &&
                (l.ServicioId == null || servicioIds.Contains(l.ServicioId.Value)) &&
                inicio < l.FechaHasta && fin > l.FechaDesde)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ListaEspera>> GetAllConDetallesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Cliente)
            .Include(l => l.Recurso)
            .Include(l => l.Servicio)
            .OrderByDescending(l => l.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<ListaEspera?> GetByIdConDetallesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Cliente)
            .Include(l => l.Recurso)
            .Include(l => l.Servicio)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }
}
