using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Core.Application.Services;

public class DisponibilidadAppService : IDisponibilidadAppService
{
    // Fija, sin configuración por tenant — simplificación de esta primera versión.
    private const int GranularidadMinutos = 15;

    private readonly IUnitOfWork _unitOfWork;

    public DisponibilidadAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TimeOnly>> GetSlotsDisponiblesAsync(
        int tenantId,
        int recursoId,
        int duracionTotalMinutos,
        DateOnly fecha,
        CancellationToken cancellationToken = default)
    {
        var horarios = await _unitOfWork.HorariosAtencion.GetByRecursoIdCrossTenantAsync(tenantId, recursoId, cancellationToken);
        var bloquesDelDia = horarios.Where(h => h.DiaSemana == fecha.DayOfWeek).ToList();

        if (bloquesDelDia.Count == 0)
            return [];

        var turnosDelDia = await _unitOfWork.Turnos.GetTurnosDelDiaCrossTenantAsync(tenantId, recursoId, fecha, cancellationToken);

        var ocupados = turnosDelDia
            .Select(t => (
                Inicio: t.FechaHoraInicio,
                Fin: t.FechaHoraInicio.AddMinutes(t.TurnoServicios.Sum(ts => ts.Servicio?.DuracionMinutos ?? 0))))
            .ToList();

        // Nota: HorarioAtencion.HoraInicio/HoraFin y Turno.FechaHoraInicio comparten la misma
        // convención "hora UTC-equivalente" (ver HorariosPage.tsx en el frontend, que hace la
        // misma conversión local↔UTC que ya hace combinarFechaYHora en TurnosPage.tsx) — acá
        // no hace falta ninguna conversión de zona horaria, son directamente comparables.
        var ahora = DateTime.UtcNow;
        var slots = new List<TimeOnly>();

        foreach (var bloque in bloquesDelDia)
        {
            var inicioMin = bloque.HoraInicio.Hour * 60 + bloque.HoraInicio.Minute;
            var finMin = bloque.HoraFin.Hour * 60 + bloque.HoraFin.Minute;

            for (var candidatoMin = inicioMin; candidatoMin + duracionTotalMinutos <= finMin; candidatoMin += GranularidadMinutos)
            {
                var candidato = new TimeOnly(candidatoMin / 60, candidatoMin % 60);
                var candidatoFin = candidato.AddMinutes(duracionTotalMinutos);

                var candidatoInicioDt = fecha.ToDateTime(candidato);
                var candidatoFinDt = fecha.ToDateTime(candidatoFin);

                var solapa = ocupados.Any(o => candidatoInicioDt < o.Fin && candidatoFinDt > o.Inicio);
                var yaPaso = candidatoInicioDt <= ahora;

                if (!solapa && !yaPaso)
                    slots.Add(candidato);
            }
        }

        return slots.OrderBy(s => s).ToList();
    }
}
