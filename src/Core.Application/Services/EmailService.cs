using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Core.Application.Services;

/// <summary>
/// Envío de emails vía la API HTTP de Resend (POST /emails), configurado por appsettings
/// (sección "Email": Password reutilizado como API key de Resend, From). Reemplaza al SMTP
/// directo (System.Net.Mail.SmtpClient contra smtp.resend.com:587): desde la red de Render
/// esa conexión TCP saliente da timeout siempre (confirmado en logs de producción), mientras
/// que la API HTTPS de Resend no tiene ese problema — mismo motivo por el que Resend la
/// recomienda por sobre SMTP para este tipo de entorno.
/// </summary>
public class EmailService : IEmailService
{
    private const string ResendApiUrl = "https://api.resend.com/emails";
    private static readonly HttpClient _httpClient = new();

    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Email:Password"];
        var from = _configuration["Email:From"];

        var payload = JsonSerializer.Serialize(new
        {
            from,
            to = new[] { destinatario },
            subject = asunto,
            html = cuerpoHtml
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Resend respondió {(int)response.StatusCode} {response.StatusCode}: {body}");
        }
    }
}
