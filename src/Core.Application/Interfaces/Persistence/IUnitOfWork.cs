using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Coordina múltiples repositorios dentro de una única transacción de base de datos.
/// Todos los repositorios expuestos aquí comparten el mismo DbContext.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IServicioRepository Servicios { get; }

    /// <summary>
    /// Persiste todos los cambios pendientes del change tracker en la base de datos.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}