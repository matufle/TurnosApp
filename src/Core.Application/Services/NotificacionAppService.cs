using Microsoft.Extensions.Configuration;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Application.Services;

public class NotificacionAppService : INotificacionAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public NotificacionAppService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
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

    public async Task ProgramarConfirmacionEmailUsuarioAsync(Usuario usuario, string token, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        var link = $"{baseUrl}/confirmar-email?token={token}";

        var notificacion = new Notificacion
        {
            TenantId = usuario.TenantId, // flujo anónimo (registro): ITenantProvider no resuelve nada acá
            Tipo = TipoNotificacion.ConfirmacionEmailUsuario,
            ClienteId = null,
            DestinatarioEmail = usuario.Email,
            Asunto = "Confirmá tu cuenta en Slotia",
            CuerpoHtml = $"Hola {usuario.Nombre}, confirmá tu cuenta para empezar a usar Slotia: " +
                         $"<a href=\"{link}\">{link}</a>. Este link vence en 48 horas.",
            ProgramadaPara = DateTime.UtcNow
        };

        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ProgramarConfirmacionEmailClienteAsync(Cliente cliente, string tenantSlug, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cliente.Email))
            return;

        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        var link = $"{baseUrl}/reservas/{tenantSlug}/confirmar-email?token={token}";

        var notificacion = new Notificacion
        {
            TenantId = cliente.TenantId, // flujo anónimo (registro): ITenantProvider no resuelve nada acá
            Tipo = TipoNotificacion.ConfirmacionEmailCliente,
            ClienteId = cliente.Id,
            DestinatarioEmail = cliente.Email,
            Asunto = "Confirmá tu cuenta",
            CuerpoHtml = $"Hola {cliente.Nombre}, confirmá tu cuenta para poder reservar turnos: " +
                         $"<a href=\"{link}\">{link}</a>. Este link vence en 48 horas.",
            ProgramadaPara = DateTime.UtcNow
        };

        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ProgramarResetPasswordEmailUsuarioAsync(Usuario usuario, string token, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        var link = $"{baseUrl}/reset-password?token={token}";

        var notificacion = new Notificacion
        {
            TenantId = usuario.TenantId, // flujo anónimo (reset): ITenantProvider no resuelve nada acá
            Tipo = TipoNotificacion.ResetPasswordUsuario,
            ClienteId = null,
            DestinatarioEmail = usuario.Email,
            Asunto = "Restablecé tu contraseña en Slotia",
            CuerpoHtml = $"Hola {usuario.Nombre}, restablecé tu contraseña en Slotia: " +
                         $"<a href=\"{link}\">{link}</a>. Este link vence en 1 hora. " +
                         "Si no lo pediste vos, ignorá este email.",
            ProgramadaPara = DateTime.UtcNow
        };

        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ProgramarResetPasswordEmailClienteAsync(Cliente cliente, string tenantSlug, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cliente.Email))
            return;

        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        var link = $"{baseUrl}/reservas/{tenantSlug}/reset-password?token={token}";

        var notificacion = new Notificacion
        {
            TenantId = cliente.TenantId, // flujo anónimo (reset): ITenantProvider no resuelve nada acá
            Tipo = TipoNotificacion.ResetPasswordCliente,
            ClienteId = cliente.Id,
            DestinatarioEmail = cliente.Email,
            Asunto = "Restablecé tu contraseña",
            CuerpoHtml = $"Hola {cliente.Nombre}, restablecé tu contraseña: " +
                         $"<a href=\"{link}\">{link}</a>. Este link vence en 1 hora. " +
                         "Si no lo pediste vos, ignorá este email.",
            ProgramadaPara = DateTime.UtcNow
        };

        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
