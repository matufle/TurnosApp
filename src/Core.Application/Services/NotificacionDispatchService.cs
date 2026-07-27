using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Application.Services;

public class NotificacionDispatchService : INotificacionDispatchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificacionDispatchService> _logger;

    public NotificacionDispatchService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<NotificacionDispatchService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<int> ProcesarPendientesAsync(CancellationToken cancellationToken = default)
    {
        var maxLote = int.Parse(_configuration["Notificaciones:MaxLotePorCiclo"] ?? "50");
        var maxIntentos = int.Parse(_configuration["Notificaciones:MaxIntentos"] ?? "5");
        var umbral = TimeSpan.FromMinutes(int.Parse(_configuration["Notificaciones:UmbralReclamoVencidoMinutos"] ?? "5"));

        var ahora = DateTime.UtcNow;
        var pendientes = await _unitOfWork.Notificaciones.ReclamarPendientesAsync(ahora, umbral, maxLote, cancellationToken);

        if (pendientes.Count == 0)
            return 0;

        foreach (var notificacion in pendientes)
        {
            try
            {
                await _emailService.EnviarAsync(
                    notificacion.DestinatarioEmail,
                    notificacion.Asunto,
                    notificacion.CuerpoHtml,
                    cancellationToken);

                notificacion.EstadoEnvio = EstadoEnvioNotificacion.Enviada;
                notificacion.EnviadaEn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notificacion.IntentosEnvio++;
                notificacion.UltimoError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;

                if (notificacion.IntentosEnvio >= maxIntentos)
                {
                    notificacion.EstadoEnvio = EstadoEnvioNotificacion.Fallida;
                    _logger.LogError(ex, "Notificacion {Id} ({Tipo}) falló definitivamente tras {Intentos} intentos.",
                        notificacion.Id, notificacion.Tipo, notificacion.IntentosEnvio);
                }
                else
                {
                    // Backoff lineal (tope 60') — se mantiene Pendiente, vuelve a ser candidata más adelante.
                    notificacion.ProgramadaPara = DateTime.UtcNow.AddMinutes(Math.Min(5 * notificacion.IntentosEnvio, 60));
                    _logger.LogWarning(ex, "Notificacion {Id} ({Tipo}) falló, reintento {Intentos}/{Max}.",
                        notificacion.Id, notificacion.Tipo, notificacion.IntentosEnvio, maxIntentos);
                }
            }

            notificacion.ModificadoEn = DateTime.UtcNow;
            // Por-fila: si el proceso muere a mitad de lote, no se pierde el progreso ya hecho.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return pendientes.Count;
    }
}
