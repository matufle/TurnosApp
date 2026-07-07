using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;
using TurnosApp.Infra.Data.Repositories;

public class TurnoRepository : GenericRepository<Turno>, ITurnoRepository
{
    public TurnoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExisteTurnoEnRangoAsync(
        int recursoId, DateTime inicio, DateTime fin,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t =>
                t.RecursoId == recursoId &&
                (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado))
            .Select(t => new
            {
                t.FechaHoraInicio,
                FechaHoraFin = t.FechaHoraInicio.AddMinutes(
                    t.TurnoServicios.Sum(ts => ts.Servicio!.DuracionMinutos))
            })
            .Where(t => inicio < t.FechaHoraFin && fin > t.FechaHoraInicio)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> GetAllConDetallesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Recurso)
            .Include(t => t.Cliente)
            .Include(t => t.TurnoServicios)
                .ThenInclude(ts => ts.Servicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<Turno?> GetByIdConDetallesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Recurso)
            .Include(t => t.Cliente)
            .Include(t => t.TurnoServicios)
                .ThenInclude(ts => ts.Servicio)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}