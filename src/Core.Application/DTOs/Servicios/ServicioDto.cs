using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.DTOs.Servicios;

/// <summary>
/// DTO de salida. Representa un Servicio tal como lo ve el cliente HTTP.
/// No expone TenantId ni propiedades de navegación de EF Core.
/// </summary>
public record ServicioDto(
    int Id,
    string Nombre,
    string? Descripcion,
    int DuracionMinutos,
    decimal Precio,
    bool Activo
);