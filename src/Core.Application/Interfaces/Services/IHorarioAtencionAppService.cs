using TurnosApp.Core.Application.DTOs.Horarios;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IHorarioAtencionAppService
{
    Task<IReadOnlyList<HorarioAtencionDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HorarioAtencionDto>> ReemplazarAsync(int recursoId, ReemplazarHorariosDto dto, CancellationToken cancellationToken = default);
}
