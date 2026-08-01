using MercadoPago.Error;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Presentation.WebAPI.Controllers;

/// <summary>
/// Recibe notificaciones de Mercado Pago. Sin [Authorize] — la seguridad acá es la
/// verificación de firma (header x-signature contra MercadoPago:WebhookSecret), no un JWT:
/// MP no tiene uno nuestro. Primer endpoint del proyecto sin el patrón [Authorize]+
/// [RequierePermiso] habitual (igual que hubiera sido el de Stripe).
/// </summary>
[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly ISuscripcionAppService _suscripcionAppService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IMercadoPagoService mercadoPagoService,
        ISuscripcionAppService suscripcionAppService,
        IUnitOfWork unitOfWork,
        ILogger<WebhooksController> logger)
    {
        _mercadoPagoService = mercadoPagoService;
        _suscripcionAppService = suscripcionAppService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Body esperado (topic "subscription_preapproval"): { "id": <notif id>, "type": "subscription_preapproval",
    /// "data": { "id": "<preapproval id>" }, ... }. MP también manda data.id como query string
    /// (?data.id=...&amp;type=...) — la validación de firma usa ESE valor, no el del body.
    /// </summary>
    [HttpPost("mercadopago")]
    public async Task<IActionResult> MercadoPago(CancellationToken cancellationToken)
    {
        NotificacionMercadoPago? notificacion;
        using (var reader = new StreamReader(Request.Body))
        {
            var json = await reader.ReadToEndAsync(cancellationToken);
            notificacion = System.Text.Json.JsonSerializer.Deserialize<NotificacionMercadoPago>(
                json, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower });
        }

        var xSignature = Request.Headers["x-signature"].FirstOrDefault();
        var xRequestId = Request.Headers["x-request-id"].FirstOrDefault();
        var dataId = Request.Query["data.id"].FirstOrDefault();

        try
        {
            _mercadoPagoService.ValidarFirmaWebhook(xSignature, xRequestId, dataId);
        }
        catch (InvalidWebhookSignatureException ex)
        {
            _logger.LogWarning("Firma de webhook de Mercado Pago inválida ({Reason})", ex.Reason);
            return Unauthorized();
        }

        if (notificacion?.Id is null || string.IsNullOrEmpty(notificacion.Data?.Id))
            return Ok(); // notificación sin la forma esperada (ej. un test ping) — no hay nada que procesar

        var notificacionId = notificacion.Id.Value.ToString();

        // Idempotencia: MP reintenta si no recibe 200 a tiempo — la misma notificación
        // (mismo id) nunca se procesa dos veces.
        var yaProcesada = await _unitOfWork.WebhookEvents.GetByNotificacionIdAsync(notificacionId, cancellationToken);
        if (yaProcesada is not null)
            return Ok();

        if (notificacion.Type == "subscription_preapproval")
        {
            await _suscripcionAppService.ActualizarDesdeNotificacionAsync(notificacion.Data.Id, cancellationToken);
        }

        await _unitOfWork.WebhookEvents.AddAsync(new WebhookEvent
        {
            NotificacionId = notificacionId,
            Tipo = notificacion.Type ?? "desconocido",
            ProcesadoEn = DateTime.UtcNow
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    private class NotificacionMercadoPago
    {
        public long? Id { get; set; }
        public string? Type { get; set; }
        public NotificacionDataMercadoPago? Data { get; set; }
    }

    private class NotificacionDataMercadoPago
    {
        public string? Id { get; set; }
    }
}
