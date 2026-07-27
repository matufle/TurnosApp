using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Workers;

/// <summary>
/// Poll periódico de la tabla Notificaciones (outbox): despacha confirmaciones/recordatorios
/// de turno y avisos de lista de espera de forma asíncrona, sin bloquear los requests HTTP
/// que los programan (ver INotificacionAppService).
/// </summary>
public class NotificacionDispatcherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificacionDispatcherWorker> _logger;
    private readonly TimeSpan _intervalo;

    public NotificacionDispatcherWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificacionDispatcherWorker> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalo = TimeSpan.FromSeconds(int.Parse(configuration["Notificaciones:PollIntervalSegundos"] ?? "90"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Pequeño delay inicial: evita competir con el resto del arranque (migraciones, etc.).
        try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Scope propio por ciclo: DbContext/IUnitOfWork son Scoped, no Singleton,
                // y esto además evita que el change tracker crezca sin límite entre ciclos.
                using var scope = _scopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<INotificacionDispatchService>();
                var procesadas = await dispatcher.ProcesarPendientesAsync(stoppingToken);

                if (procesadas > 0)
                    _logger.LogInformation("NotificacionDispatcherWorker procesó {Count} notificaciones.", procesadas);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Un ciclo entero fallando (ej. DB caída) no debe tumbar el worker para
                // siempre: BackgroundService no se reinicia solo tras una excepción no
                // capturada en ExecuteAsync, así que este catch es lo que permite
                // auto-recuperarse en el próximo ciclo.
                _logger.LogError(ex, "Error inesperado en el ciclo de NotificacionDispatcherWorker.");
            }

            try { await Task.Delay(_intervalo, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
