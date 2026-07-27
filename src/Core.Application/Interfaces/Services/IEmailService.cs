namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IEmailService
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken = default);
}
