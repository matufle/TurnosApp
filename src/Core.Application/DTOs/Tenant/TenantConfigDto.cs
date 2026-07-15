using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Tenant
{
    public record TenantConfigDto(
    string Nombre,
    string ColorPrimario,
    bool PermiteReservasPublicas
);
}
