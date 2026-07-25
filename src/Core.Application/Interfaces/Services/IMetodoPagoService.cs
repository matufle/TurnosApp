using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.MetodosPago;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IMetodoPagoService
{
    Task<MetodoPagoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MetodoPagoDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MetodoPagoDto> CreateAsync(CreateMetodoPagoDto dto, CancellationToken cancellationToken = default);

    Task<MetodoPagoDto> UpdateAsync(int id, UpdateMetodoPagoDto dto, CancellationToken cancellationToken = default);

    Task<MetodoPagoDto> DesactivarAsync(int id, CancellationToken cancellationToken = default);
}
