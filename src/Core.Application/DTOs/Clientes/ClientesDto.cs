using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Clientes;

/// <summary>
/// DTO de salida. Expone NotasAdicionales como string plano;
/// el JSON interno de DatosEspecificosJson queda encapsulado aquí
/// hasta que el frontend requiera un modelo tipado.
/// </summary>
public record ClienteDto(
    int Id,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? NotasAdicionales
);