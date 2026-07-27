using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Tenant
{
    public record TenantConfigDto(
    string Nombre,
    string Slug,
    string ColorPrimario,
    bool PermiteReservasPublicas,
    bool PermiteSolapamiento
);
}
