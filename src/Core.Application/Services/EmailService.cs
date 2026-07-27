using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Core.Application.Services;

/// <summary>
/// Envío de emails vía SMTP plano (System.Net.Mail), configurado por appsettings
/// (sección "Email"): Host, Port, Usuario, Password, From, UseSsl. Sin dependencia de
/// NuGet nueva — mismo espíritu que JwtTokenService leyendo su config vía IConfiguration.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:Host"];
        var port = int.Parse(_configuration["Email:Port"] ?? "587");
        var usuario = _configuration["Email:Usuario"];
        var password = _configuration["Email:Password"];
        var from = _configuration["Email:From"] ?? usuario;
        var useSsl = bool.Parse(_configuration["Email:UseSsl"] ?? "true");

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(usuario, password),
            EnableSsl = useSsl
        };

        using var mensaje = new MailMessage(from!, destinatario, asunto, cuerpoHtml)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mensaje, cancellationToken);
    }
}
