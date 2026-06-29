using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Servicios;

/// <summary>
/// DTO de entrada para creación. El TenantId no es recibido del cliente —
/// lo asigna la capa de Application a partir del ITenantProvider.
/// </summary>
public record CreateServicioDto(
    string Nombre,
    string? Descripcion,
    int DuracionMinutos,
    decimal Precio
);