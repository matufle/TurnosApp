using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface INotificacionRepository : IRepository<Notificacion>
{
    /// <summary>
    /// Reclama (cross-tenant, uso exclusivo del worker) un lote de notificaciones pendientes
    /// y vencidas, marcándolas atómicamente antes de devolverlas para evitar doble envío
    /// si un ciclo de poll se solapa con el anterior.
    /// </summary>
    Task<IReadOnlyList<Notificacion>> ReclamarPendientesAsync(
        DateTime ahora,
        TimeSpan umbralReclamoVencido,
        int maxLote,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notificaciones aún pendientes ligadas a un turno (tenant-filtrado, se llama desde
    /// un request HTTP normal al cancelar el turno).
    /// </summary>
    Task<IReadOnlyList<Notificacion>> GetPendientesDeTurnoAsync(
        int turnoId,
        CancellationToken cancellationToken = default);
}
