namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IDisponibilidadAppService
{
    /// <summary>
    /// Horarios de inicio disponibles para un recurso, en una fecha dada, para un turno
    /// de la duración indicada — dentro de los bloques de HorarioAtencion del recurso ese
    /// día de semana, descartando los que se solapan con turnos ya cargados o que ya pasaron
    /// (si la fecha es hoy).
    /// </summary>
    Task<IReadOnlyList<TimeOnly>> GetSlotsDisponiblesAsync(
        int tenantId,
        int recursoId,
        int duracionTotalMinutos,
        DateOnly fecha,
        CancellationToken cancellationToken = default);
}
