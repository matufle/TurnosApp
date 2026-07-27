using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

public interface IRecursoRepository : IRepository<Recurso>
{
    Task<Recurso?> GetByUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>Cross-tenant a propósito: catálogo público, sin JWT que resolver.</summary>
    Task<IReadOnlyList<Recurso>> GetActivosCrossTenantAsync(int tenantId, CancellationToken cancellationToken = default);
}
