using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Application.Services;

public class NotificacionAppService : INotificacionAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificacionAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ProgramarConfirmacionAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(turno.Cliente?.Email))
            return;

        var recursoNombre = turno.Recurso?.Nombre ?? "el recurso solicitado";
        var servicios = string.Join(", ", turno.TurnoServicios.Select(ts => ts.Servicio?.Nombre ?? string.Empty));

        var notificacion = new Notificacion
        {
            Tipo = TipoNotificacion.ConfirmacionTurno,
            TurnoId = turno.Id,
            ClienteId = turno.ClienteId,
            DestinatarioEmail = turno.Cliente.Email,
            Asunto = "Confirmamos tu turno",
            CuerpoHtml = $"Hola {turno.Cliente.Nombre}, confirmamos tu turno con {recursoNombre} " +
                         $"el {turno.FechaHoraInicio:dd/MM/yyyy HH:mm}" +
                         (servicios.Length > 0 ? $" para {servicios}." : "."),
            ProgramadaPara = DateTime.UtcNow
        };

        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ProgramarRecordatorioAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(turno.Cliente?.Email))
            return;

        var programadaPara = turno.FechaHoraInicio.AddHours(-24);

        // Sin "24hs antes" con sentido (turno reservado para hoy o mañana): la confirmación
        // ya cubre el aviso, no creamos una fila que no aportaría nada.
        if (programadaPara <= DateTime.UtcNow)
            return;

        var recursoNombre = turno.Recurso?.Nombre ?? "el recurso solicitado";

        var notificacion = new Notificacion
        {
            Tipo = TipoNotificacion.RecordatorioTurno,
            TurnoId = turno.Id,
            ClienteId = turno.ClienteId,
            DestinatarioEmail = turno.Cliente.Email,
            Asunto = "Recordatorio de tu turno",
            CuerpoHtml = $"Hola {turno.Cliente.Nombre}, te recordamos tu turno con {recursoNombre} " +
                         $"mañana {turno.FechaHoraInicio:dd/MM/yyyy HH:mm}.",
            ProgramadaPara = programadaPara
        };

        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ProgramarListaEsperaAsync(ListaEspera entrada, Turno turnoLiberado, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entrada.Cliente?.Email))
            return;

        var recursoNombre = turnoLiberado.Recurso?.Nombre ?? "el recurso solicitado";

        var notificacion = new Notificacion
        {
            Tipo = TipoNotificacion.ListaEspera,
            TurnoId = turnoLiberado.Id,
            ListaEsperaId = entrada.Id,
            ClienteId = entrada.ClienteId,
            DestinatarioEmail = entrada.Cliente.Email,
            Asunto = "¡Se liberó un turno que estabas esperando!",
            CuerpoHtml = $"Hola {entrada.Cliente.Nombre}, se liberó un horario con {recursoNombre} " +
                         $"el {turnoLiberado.FechaHoraInicio:dd/MM/yyyy HH:mm}. " +
                         "Contactanos para reservarlo antes de que se ocupe.",
            ProgramadaPara = DateTime.UtcNow
        };

        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelarPendientesDeTurnoAsync(int turnoId, CancellationToken cancellationToken = default)
    {
        var pendientes = await _unitOfWork.Notificaciones.GetPendientesDeTurnoAsync(turnoId, cancellationToken);

        if (pendientes.Count == 0)
            return;

        foreach (var notificacion in pendientes)
        {
            notificacion.EstadoEnvio = EstadoEnvioNotificacion.Cancelada;
            notificacion.ModificadoEn = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
