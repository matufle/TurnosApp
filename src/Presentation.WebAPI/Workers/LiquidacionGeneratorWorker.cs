using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Workers;

/// <summary>
/// Poll periódico que genera liquidaciones de comisión por profesional cuando el período
/// configurado por tenant (semanal/quincenal/mensual) cierra. Los períodos se miden en días,
/// por eso el intervalo de poll es mucho más largo que el de NotificacionDispatcherWorker.
/// </summary>
public class LiquidacionGeneratorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiquidacionGeneratorWorker> _logger;
    private readonly TimeSpan _intervalo;

    public LiquidacionGeneratorWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<LiquidacionGeneratorWorker> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalo = TimeSpan.FromSeconds(int.Parse(configuration["Liquidaciones:PollIntervalSegundos"] ?? "3600"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Pequeño delay inicial: evita competir con el resto del arranque (migraciones, etc.).
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Scope propio por ciclo: DbContext/IUnitOfWork son Scoped, no Singleton.
                using var scope = _scopeFactory.CreateScope();
                var generador = scope.ServiceProvider.GetRequiredService<ILiquidacionGeneratorService>();
                await generador.GenerarPendientesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Un ciclo entero fallando no debe tumbar el worker para siempre: BackgroundService
                // no se reinicia solo tras una excepción no capturada en ExecuteAsync.
                _logger.LogError(ex, "Error inesperado en el ciclo de LiquidacionGeneratorWorker.");
            }

            try { await Task.Delay(_intervalo, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
