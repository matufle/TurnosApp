namespace TurnosApp.Core.Application.Interfaces.Services;

/// <summary>Resultado mínimo de crear un preapproval — no exponemos el tipo del SDK hacia afuera.</summary>
public record PreapprovalCreado(string Id, string InitPoint);

/// <summary>
/// Wrapper fino sobre el SDK oficial de Mercado Pago (mercadopago-sdk). No toca la base de
/// datos (la persistencia de MercadoPagoPreapprovalId/etc. es responsabilidad de
/// ISuscripcionAppService) — mismo criterio que tenía IStripeService.
/// </summary>
public interface IMercadoPagoService
{
    /// <summary>
    /// Crea un preapproval (suscripción) "sin plan asociado" en estado pending — MP no
    /// requiere un recurso de catálogo previo (a diferencia de Stripe Price/Product), el
    /// monto/moneda/frecuencia se mandan inline. Devuelve el id del preapproval y la URL de
    /// checkout hospedada (init_point en prod, sandbox_init_point con un Access Token TEST-).
    /// </summary>
    Task<PreapprovalCreado> CrearPreapprovalAsync(
        string payerEmail,
        string reason,
        decimal montoMensual,
        string backUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Cancela un preapproval existente (PUT status=cancelled) — no hay portal de autogestión del pagador.</summary>
    Task CancelarPreapprovalAsync(string preapprovalId, CancellationToken cancellationToken = default);

    /// <summary>Consulta el estado actual de un preapproval directo en la API (para reconciliar, no solo confiar en el webhook).</summary>
    Task<string> ObtenerEstadoAsync(string preapprovalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica la firma x-signature del webhook contra MercadoPago:WebhookSecret.
    /// Lanza MercadoPago.Error.InvalidWebhookSignatureException si no es válida.
    /// </summary>
    void ValidarFirmaWebhook(string? xSignature, string? xRequestId, string? dataId);
}
