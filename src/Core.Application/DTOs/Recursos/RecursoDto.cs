using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.DTOs.Recursos;

public record RecursoDto(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Activo,
    string ColorHex,
    int? UsuarioId,
    string? UsuarioNombre
);