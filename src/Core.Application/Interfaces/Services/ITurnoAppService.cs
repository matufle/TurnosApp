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
}
