namespace TurnosApp.Core.Application.Interfaces.Services;

public interface INotificacionDispatchService
{
    /// <summary>
    /// Reclama y despacha un lote de notificaciones pendientes y vencidas. Devuelve
    /// cuántas se procesaron (enviadas o marcadas como fallidas/reintento).
    /// </summary>
    Task<int> ProcesarPendientesAsync(CancellationToken cancellationToken = default);
}
