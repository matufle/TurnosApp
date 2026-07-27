using TurnosApp.Core.Application.DTOs.ListaEspera;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IListaEsperaAppService
{
    Task<ListaEsperaDto> CrearAsync(CrearListaEsperaDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ListaEsperaDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ListaEsperaDto> CancelarAsync(int id, CancellationToken cancellationToken = default);
}
