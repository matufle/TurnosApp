using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Core.Application.Services;

/// <summary>
/// Verifica tokens de Cloudflare Turnstile contra /siteverify. HttpClient estático
/// (no IHttpClientFactory, sin agregar el paquete Microsoft.Extensions.Http) — mismo
/// espíritu que EmailService usando System.Net.Mail directo.
/// </summary>
public class TurnstileService : ITurnstileService
{
    private const string SiteVerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
    private static readonly HttpClient _httpClient = new();

    private readonly IConfiguration _configuration;

    public TurnstileService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> VerificarAsync(string? token, CancellationToken cancellationToken = default)
    {
        var secretKey = _configuration["Turnstile:SecretKey"];

        // Sin secret configurado (clon local sin appsettings.Development.json propio):
        // no bloqueamos el registro, a diferencia de un token vacío o rechazado por Cloudflare.
        if (string.IsNullOrWhiteSpace(secretKey))
            return true;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = secretKey,
            ["response"] = token
        });

        try
        {
            using var response = await _httpClient.PostAsync(SiteVerifyUrl, form, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return false;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return json.RootElement.TryGetProperty("success", out var success) && success.GetBoolean();
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested is false)
        {
            // Cloudflare caído o timeout: fail closed, no dejamos pasar el registro sin verificar.
            return false;
        }
    }
}
