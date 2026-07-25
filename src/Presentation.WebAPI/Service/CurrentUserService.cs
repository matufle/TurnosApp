using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Presentation.WebAPI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    // Cacheado en la instancia: es Scoped (una por request), evita repetir la consulta
    // si más de un punto del pipeline pregunta por los permisos del usuario actual.
    private Usuario? _usuarioCacheado;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
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

    public int GetCurrentUsuarioId()
    {
        // Claim custom (no "sub"): el JwtBearerHandler remapea "sub" a ClaimTypes.NameIdentifier
        // por el inbound claim mapping default, así que lo leemos de vuelta con el mismo nombre custom.
        var usuarioClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UsuarioId")?.Value;

        if (string.IsNullOrEmpty(usuarioClaim) || !int.TryParse(usuarioClaim, out var usuarioId))
        {
            throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");
        }

        return usuarioId;
    }

    public async Task<Permiso> GetCurrentPermisosAsync(CancellationToken cancellationToken = default)
    {
        var usuario = await ResolverUsuarioAsync(cancellationToken);
        return usuario.Rol.Permisos;
    }

    public async Task<int?> GetCurrentRecursoIdAsync(CancellationToken cancellationToken = default)
    {
        var usuarioId = GetCurrentUsuarioId();
        var recurso = await _unitOfWork.Recursos.GetByUsuarioIdAsync(usuarioId, cancellationToken);
        return recurso?.Id;
    }

    private async Task<Usuario> ResolverUsuarioAsync(CancellationToken cancellationToken)
    {
        if (_usuarioCacheado is not null)
            return _usuarioCacheado;

        var usuarioId = GetCurrentUsuarioId();
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new UnauthorizedAccessException("El usuario autenticado ya no existe.");

        // Un JWT con "recordarme" vive hasta 30 días: si el Admin desactivó a este usuario
        // en el medio, esto corta el acceso a cualquier endpoint con [RequierePermiso] de
        // inmediato, sin esperar a que expire el token.
        if (!usuario.Activo)
            throw new UnauthorizedAccessException("Tu usuario fue desactivado.");

        _usuarioCacheado = usuario;
        return usuario;
    }
}
