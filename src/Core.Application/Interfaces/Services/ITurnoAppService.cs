using System;
using System.Collections.Generic;
using System.Text;

using TurnosApp.Core.Application.DTOs.Turnos;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ITurnoAppService
{
    Task<TurnoDto> CrearTurnoAsync(
        CrearTurnoDto dto,
        CancellationToken cancellationToken = default);
}
