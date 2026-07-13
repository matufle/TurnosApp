using System.Security.Claims;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentTenantId()
    {
        // Leemos el Claim del TenantId que viene adentro del JWT
        var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantClaim) || !int.TryParse(tenantClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("No se pudo identificar el Tenant del usuario actual.");
        }

        return tenantId;
    }
}