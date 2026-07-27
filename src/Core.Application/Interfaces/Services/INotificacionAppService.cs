using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface INotificacionAppService
{
    /// <summary>
    /// Programa el email de confirmación de un turno recién creado, para envío
    /// cuasi-inmediato por el worker. No-op si el cliente no tiene email cargado.
    /// </summary>
    Task ProgramarConfirmacionAsync(Turno turno, CancellationToken cancellationToken = default);

    /// <summary>
    /// Programa el recordatorio de un turno para 24hs antes de su inicio. No crea nada
    /// si ese momento ya pasó (turno reservado para hoy o mañana) ni si falta el email.
    /// </summary>
    Task ProgramarRecordatorioAsync(Turno turno, CancellationToken cancellationToken = default);

    /// <summary>
    /// Programa el aviso de "se liberó un turno" a una entrada de lista de espera.
    /// </summary>
    Task ProgramarListaEsperaAsync(ListaEspera entrada, Turno turnoLiberado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela cualquier notificación aún Pendiente ligada a un turno (ej. su recordatorio
    /// de 24hs) para que el worker nunca llegue a despacharla.
    /// </summary>
    Task CancelarPendientesDeTurnoAsync(int turnoId, CancellationToken cancellationToken = default);
}
