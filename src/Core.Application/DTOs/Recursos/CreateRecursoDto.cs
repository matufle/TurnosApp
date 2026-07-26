using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Recursos;

public record CreateRecursoDto(
    string Nombre,
    string? Descripcion,
    string? ColorHex,
    int? UsuarioId
);