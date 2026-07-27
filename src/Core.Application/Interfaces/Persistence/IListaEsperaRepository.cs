using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface IListaEsperaRepository : IRepository<ListaEspera>
{
    /// <summary>
    /// Entradas activas del Recurso indicado cuya ventana deseada se solapa con el rango
    /// [inicio, fin) recién liberado, y cuyo ServicioId (si lo tienen) está entre los servicios
    /// del turno cancelado. Mismo patrón de solapamiento que TurnoRepository.ExisteTurnoEnRangoAsync.
    /// </summary>
    Task<IReadOnlyList<ListaEspera>> BuscarCoincidenciasAsync(
        int recursoId,
        IReadOnlyCollection<int> servicioIds,
        DateTime inicio,
        DateTime fin,
        CancellationToken cancellationToken = default);

    /// <summary>GetAllAsync con Cliente/Recurso/Servicio incluidos, para mapear el listado a DTO.</summary>
    Task<IReadOnlyList<ListaEspera>> GetAllConDetallesAsync(CancellationToken cancellationToken = default);

    /// <summary>GetByIdAsync con Cliente/Recurso/Servicio incluidos, para mapear a DTO.</summary>
    Task<ListaEspera?> GetByIdConDetallesAsync(int id, CancellationToken cancellationToken = default);
}
