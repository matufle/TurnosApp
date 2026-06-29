using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio de Tenant. Lee sin Global Query Filter
/// (Tenant no hereda de TenantEntity).
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
}
