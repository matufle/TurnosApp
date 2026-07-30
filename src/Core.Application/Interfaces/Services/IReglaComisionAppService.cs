using TurnosApp.Core.Application.DTOs.ReglasComision;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IReglaComisionAppService
{
    Task<IReadOnlyList<ReglaComisionDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default);

    Task<ReglaComisionDto> CreateAsync(CreateReglaComisionDto dto, CancellationToken cancellationToken = default);

    Task<ReglaComisionDto> UpdateAsync(int id, UpdateReglaComisionDto dto, CancellationToken cancellationToken = default);
}
