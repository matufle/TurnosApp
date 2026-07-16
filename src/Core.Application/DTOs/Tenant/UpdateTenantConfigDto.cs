using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Tenant
{
    public record UpdateTenantConfigDto(
        string ColorPrimario,
        bool PermiteReservasPublicas,
        bool PermiteSolapamiento
    );
}
