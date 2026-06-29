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
    /// <summary>
    /// Determina si existe algún turno confirmado o pendiente para el recurso dado
    /// cuyo rango [inicio, fin) se solapa con el rango propuesto.
    /// Fórmula de solapamiento de intervalos: inicioNuevo < finExistente
    ///                                    AND finNuevo   > inicioExistente
    /// </summary>
    Task<bool> ExisteTurnoEnRangoAsync(
        int recursoId,
        DateTime inicio,
        DateTime fin,
        CancellationToken cancellationToken = default);
}
