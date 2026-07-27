using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Services;

public class CurrentClienteService : ICurrentClienteService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentClienteService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentClienteId()
    {
        // Claim custom (no "sub"), calco de CurrentUserService.GetCurrentUsuarioId(): un
        // token de Usuario nunca trae "ClienteId", así que esto falla cerrado por diseño
        // si algún token de staff intenta pegarle a un endpoint de cliente.
        var clienteClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ClienteId")?.Value;

        if (string.IsNullOrEmpty(clienteClaim) || !int.TryParse(clienteClaim, out var clienteId))
        {
            throw new UnauthorizedAccessException("No se pudo identificar al cliente autenticado.");
        }

        return clienteId;
    }
}
