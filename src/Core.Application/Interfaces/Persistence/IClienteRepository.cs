using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio específico de Cliente.
/// Placeholder para queries complejas futuras:
/// ej. BuscarPorEmailAsync, GetClientesActivosConTurnosPendientesAsync.
/// </summary>
public interface IClienteRepository : IRepository<Cliente>
{
    /// <summary>
    /// Cross-tenant a propósito: se llama desde el flujo anónimo de registro/login de
    /// cliente, sin JWT del que ITenantProvider pueda resolver un TenantId.
    /// </summary>
    Task<Cliente?> GetByTenantYEmailAsync(int tenantId, string emailNormalizado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cross-tenant a propósito, mismo motivo que GetByTenantYEmailAsync: la confirmación
    /// de email de Cliente ocurre sin JWT.
    /// </summary>
    Task<Cliente?> GetByTenantYTokenConfirmacionAsync(int tenantId, string token, CancellationToken cancellationToken = default);
}