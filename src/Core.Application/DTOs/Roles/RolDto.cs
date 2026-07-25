using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.DTOs.Roles;

/// <summary>
/// DTO de salida. Los permisos se exponen como nombres legibles, no como el bitmask crudo.
/// </summary>
public record RolDto(
    int Id,
    string Nombre,
    bool EsSistema,
    IReadOnlyList<string> Permisos
);
