using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface ICobroRepository : IRepository<Cobro>
{
    Task<IReadOnlyList<Cobro>> GetAllByTurnoIdAsync(int turnoId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Cobro> Items, int TotalCount, decimal SumaPrecioFinal, decimal SumaComision)> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? busqueda,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default);
}
