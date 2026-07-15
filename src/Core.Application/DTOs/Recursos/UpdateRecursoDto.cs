using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Recursos;

public record UpdateRecursoDto(
    string Nombre,
    string? Descripcion,
    bool Activo,
    string? ColorHex
);