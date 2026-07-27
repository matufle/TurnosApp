using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio específico de Servicio.
/// Extiende el genérico para permitir queries complejas a futuro
/// (ej: GetServiciosActivosAsync, GetByRecursoAsync).
/// </summary>
public interface IServicioRepository : IRepository<Servicio>
{
    /// <summary>Cross-tenant a propósito: catálogo público, sin JWT que resolver.</summary>
    Task<IReadOnlyList<Servicio>> GetActivosCrossTenantAsync(int tenantId, CancellationToken cancellationToken = default);

    /// <summary>Cross-tenant a propósito: resolver duración para el cálculo de disponibilidad pública.</summary>
    Task<Servicio?> GetByIdCrossTenantAsync(int tenantId, int servicioId, CancellationToken cancellationToken = default);
}
