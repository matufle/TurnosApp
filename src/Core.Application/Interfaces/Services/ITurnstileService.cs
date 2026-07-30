namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ITurnstileService
{
    /// <summary>
    /// Valida un token de Cloudflare Turnstile contra la API de siteverify.
    /// Devuelve false ante cualquier fallo (token vacío, rechazo de Cloudflare, error de red).
    /// </summary>
    Task<bool> VerificarAsync(string? token, CancellationToken cancellationToken = default);
}
