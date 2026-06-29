using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Contrato genérico de persistencia asíncrona.
/// T debe ser una clase para garantizar tipos de referencia válidos con EF Core.
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtiene una entidad por su clave primaria.
    /// Retorna null si no existe o no pertenece al tenant actual.
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todas las entidades del tenant actual (Global Query Filter aplicado).
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega la entidad al change tracker. Persiste al llamar SaveChangesAsync.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca la entidad como modificada en el change tracker.
    /// Persiste al llamar SaveChangesAsync.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Marca la entidad para eliminación en el change tracker.
    /// Persiste al llamar SaveChangesAsync.
    /// </summary>
    void Delete(T entity);
}