using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio específico de Turno.
/// Extiende el genérico con la query de solapamiento, que requiere
/// lógica de rango de fechas que no puede expresarse en el contrato genérico.
/// </summary>
public interface ITurnoRepository : IRepository<Turno>
{
    Task<bool> ExisteTurnoEnRangoAsync(int recursoId, DateTime inicio, DateTime fin, CancellationToken cancellationToken = default);

    // Nuevos: traen las navegaciones necesarias para mapear a TurnoDto
    Task<IReadOnlyList<Turno>> GetAllConDetallesAsync(CancellationToken cancellationToken = default);
    Task<Turno?> GetByIdConDetallesAsync(int id, CancellationToken cancellationToken = default);
}
