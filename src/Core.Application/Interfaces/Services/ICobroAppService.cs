using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Cobros;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ICobroAppService
{
    Task<CobroDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CobroDto>> GetAllByTurnoIdAsync(int turnoId, CancellationToken cancellationToken = default);

    Task<CobroDto> CrearCobroAsync(CreateCobroDto dto, CancellationToken cancellationToken = default);

    Task<CobroDto> ActualizarCobroAsync(int id, UpdateCobroDto dto, CancellationToken cancellationToken = default);

    Task<HistorialCobrosDto> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? busqueda,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default);
}
