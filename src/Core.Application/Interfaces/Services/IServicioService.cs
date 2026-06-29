using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Servicios;

namespace TurnosApp.Core.Application.Interfaces.Services;

/// <summary>
/// Contrato del caso de uso ABM de Servicios.
/// La capa de presentación depende de esta abstracción, nunca de la implementación.
/// </summary>
public interface IServicioService
{
    Task<ServicioDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServicioDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServicioDto> CreateAsync(CreateServicioDto dto, CancellationToken cancellationToken = default);

    Task<ServicioDto> UpdateAsync(int id, UpdateServicioDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
