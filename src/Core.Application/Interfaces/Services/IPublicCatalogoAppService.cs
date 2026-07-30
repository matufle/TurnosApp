using TurnosApp.Core.Application.DTOs.Public;
using TurnosApp.Core.Application.DTOs.Servicios;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IPublicCatalogoAppService
{
    Task<IReadOnlyList<ServicioDto>> GetServiciosAsync(string tenantSlug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecursoPublicoDto>> GetRecursosAsync(string tenantSlug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetDisponibilidadAsync(
        string tenantSlug, int recursoId, IReadOnlyList<int> servicioIds, DateOnly fecha, CancellationToken cancellationToken = default);
}
