using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Tenants;

public record CreateTenantDto(
    string Nombre,
    string Slug,
    bool PermiteSolapamiento,
    string ColorPrimario,
    bool PermiteReservasPublicas
);
