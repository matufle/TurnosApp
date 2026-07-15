using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Tenants;

public record TenantDto(
    int Id,
    string Nombre,
    string Slug,
    bool PermitirSolapamiento,
    bool Activo,
    DateTime FechaAlta,
    string ColorPrimario,
    bool PermiteReservasPublicas
);