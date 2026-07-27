using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Turnos;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ITurnoAppService
{
    Task<IReadOnlyList<TurnoDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TurnoDto> CrearTurnoAsync(CrearTurnoDto dto, CancellationToken cancellationToken = default);
    Task CancelarTurnoAsync(int id, CancellationToken cancellationToken = default);
    Task<TurnoDto> CambiarEstadoTurnoAsync(int id, CambiarEstadoTurnoDto dto, CancellationToken cancellationToken = default);

    /// <summary>Turnos del cliente autenticado (self-service).</summary>
    Task<IReadOnlyList<TurnoDto>> GetMisTurnosAsync(int clienteId, CancellationToken cancellationToken = default);

    /// <summary>Cancelación self-service: valida que el turno le pertenezca al cliente antes de cancelar.</summary>
    Task CancelarPropioAsync(int clienteId, int turnoId, CancellationToken cancellationToken = default);

    /// <summary>Creación self-service: el ClienteId sale del JWT, nunca del body.</summary>
    Task<TurnoDto> CrearTurnoPublicoAsync(int clienteId, CrearTurnoPublicoDto dto, CancellationToken cancellationToken = default);
}
