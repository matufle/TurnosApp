using TurnosApp.Core.Application.DTOs.Public;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IPublicAppService
{
    /// <summary>
    /// Resuelve un tenant por su slug para el punto de entrada público (self-service).
    /// Lanza NotFoundException si el slug no existe, el tenant está inactivo, o no tiene
    /// habilitadas las reservas públicas — mismas tres condiciones, mismo 404: no revelamos
    /// si un slug existe pero está deshabilitado.
    /// </summary>
    Task<TenantPublicoDto> ResolverTenantPorSlugAsync(string slug, CancellationToken cancellationToken = default);
}
