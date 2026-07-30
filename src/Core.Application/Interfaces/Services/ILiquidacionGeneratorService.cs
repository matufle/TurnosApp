namespace TurnosApp.Core.Application.Interfaces.Services;

/// <summary>
/// Genera liquidaciones de comisión por profesional. Cross-tenant a propósito (ver
/// implementación) — pensado para ser llamado por LiquidacionGeneratorWorker (todos los
/// tenants) o por el endpoint manual de generación (un tenant puntual, el del caller).
/// </summary>
public interface ILiquidacionGeneratorService
{
    /// <summary>Recorre todos los tenants activos y genera las liquidaciones pendientes de cada uno.</summary>
    Task GenerarPendientesAsync(CancellationToken cancellationToken = default);

    /// <summary>Genera las liquidaciones pendientes de un único tenant.</summary>
    Task GenerarParaTenantAsync(int tenantId, CancellationToken cancellationToken = default);
}
