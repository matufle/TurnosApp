using TurnosApp.Core.Application.DTOs.AdelantosProfesional;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IAdelantoProfesionalAppService
{
    Task<IReadOnlyList<AdelantoProfesionalDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default);

    Task<AdelantoProfesionalDto> CreateAsync(CreateAdelantoProfesionalDto dto, CancellationToken cancellationToken = default);
}
