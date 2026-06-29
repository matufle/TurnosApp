using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Servicios;

/// <summary>
/// DTO de entrada para actualización. Incluye todos los campos editables.
/// El Id viaja en la URL (parámetro de ruta), no en el body.
/// </summary>
public record UpdateServicioDto(
    string Nombre,
    string? Descripcion,
    int DuracionMinutos,
    decimal Precio,
    bool Activo
);