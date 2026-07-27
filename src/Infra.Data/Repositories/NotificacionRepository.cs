using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class NotificacionRepository : GenericRepository<Notificacion>, INotificacionRepository
{
    public NotificacionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Notificacion>> ReclamarPendientesAsync(
        DateTime ahora,
        TimeSpan umbralReclamoVencido,
        int maxLote,
        CancellationToken cancellationToken = default)
    {
        var umbral = ahora - umbralReclamoVencido;

        // Cross-tenant a propósito: el worker no tiene un tenant "actual" resolvible
        // (no hay HttpContext/JWT dentro de su propio scope).
        var idsCandidatos = await _dbSet
            .IgnoreQueryFilters()
            .Where(n => n.EstadoEnvio == EstadoEnvioNotificacion.Pendiente
                     && n.ProgramadaPara <= ahora
                     && (n.ReclamadaEn == null || n.ReclamadaEn < umbral))
            .OrderBy(n => n.ProgramadaPara)
            .Take(maxLote)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (idsCandidatos.Count == 0)
            return [];

        var runId = Guid.NewGuid();

        await _dbSet
            .IgnoreQueryFilters()
            .Where(n => idsCandidatos.Contains(n.Id) && n.EstadoEnvio == EstadoEnvioNotificacion.Pendiente)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.ReclamadaEn, ahora)
                .SetProperty(n => n.ReclamadaPorRunId, runId), cancellationToken);

        // Tracked (sin AsNoTracking): el dispatcher muta estas filas en memoria y las guarda.
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(n => n.ReclamadaPorRunId == runId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notificacion>> GetPendientesDeTurnoAsync(
        int turnoId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.TurnoId == turnoId && n.EstadoEnvio == EstadoEnvioNotificacion.Pendiente)
            .ToListAsync(cancellationToken);
    }
}
