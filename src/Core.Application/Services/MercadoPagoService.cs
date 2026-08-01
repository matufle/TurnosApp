using Microsoft.Extensions.Configuration;
using MercadoPago.Client;
using MercadoPago.Client.Preapproval;
using MercadoPago.Webhook;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Core.Application.Services;

/// <summary>
/// El SDK de Mercado Pago guarda el Access Token en un config global estático
/// (MercadoPagoConfig.AccessToken) — para no depender de estado mutable compartido (mismo
/// criterio que StripeClient por instancia), lo pasamos por-request vía RequestOptions.AccessToken
/// en cada llamada en vez de setear el estático.
/// </summary>
public class MercadoPagoService : IMercadoPagoService
{
    private readonly IConfiguration _configuration;
    private readonly PreapprovalClient _preapprovalClient;

    public MercadoPagoService(IConfiguration configuration)
    {
        _configuration = configuration;
        _preapprovalClient = new PreapprovalClient();
    }

    private RequestOptions Credenciales() => new()
    {
        AccessToken = _configuration["MercadoPago:AccessToken"]
    };

    public async Task<PreapprovalCreado> CrearPreapprovalAsync(
        string payerEmail,
        string reason,
        decimal montoMensual,
        string backUrl,
        CancellationToken cancellationToken = default)
    {
        var currency = _configuration["MercadoPago:Currency"] ?? "ARS";

        var request = new PreapprovalCreateRequest
        {
            PayerEmail = payerEmail,
            Reason = reason,
            BackUrl = backUrl,
            Status = "pending", // sin card_token_id: el pagador define el medio de pago en el checkout hospedado
            AutoRecurring = new PreApprovalAutoRecurringCreateRequest
            {
                Frequency = 1,
                FrequencyType = "months",
                TransactionAmount = montoMensual,
                CurrencyId = currency
            }
        };

        var preapproval = await _preapprovalClient.CreateAsync(request, Credenciales(), cancellationToken);

        // Con un Access Token de prueba (TEST-...) MP solo completa SandboxInitPoint;
        // InitPoint queda vacío hasta pasar a credenciales de producción.
        var initPoint = !string.IsNullOrEmpty(preapproval.InitPoint)
            ? preapproval.InitPoint
            : preapproval.SandboxInitPoint;

        return new PreapprovalCreado(preapproval.Id, initPoint);
    }

    public async Task CancelarPreapprovalAsync(string preapprovalId, CancellationToken cancellationToken = default)
    {
        await _preapprovalClient.UpdateAsync(
            preapprovalId,
            new PreapprovalUpdateRequest { Status = "cancelled" },
            Credenciales(),
            cancellationToken);
    }

    public async Task<string> ObtenerEstadoAsync(string preapprovalId, CancellationToken cancellationToken = default)
    {
        var preapproval = await _preapprovalClient.GetAsync(preapprovalId, Credenciales(), cancellationToken);
        return preapproval.Status;
    }

    public void ValidarFirmaWebhook(string? xSignature, string? xRequestId, string? dataId)
    {
        var secret = _configuration["MercadoPago:WebhookSecret"];
        WebhookSignatureValidator.Validate(xSignature, xRequestId, dataId, secret);
    }
}
