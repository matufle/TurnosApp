using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Domain.Common;
//Esta clase es la que usamos para identificar a las entidades que pertenecen a un Tenant específico
public abstract class TenantEntity : BaseEntity
{
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}