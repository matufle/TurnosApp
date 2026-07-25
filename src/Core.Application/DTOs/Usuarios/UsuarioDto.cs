using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.DTOs.Usuarios;

/// <summary>
/// DTO de salida. No expone PasswordHash ni TenantId.
/// </summary>
public record UsuarioDto(
    int Id,
    string Nombre,
    string Email,
    int RolId,
    string RolNombre,
    bool Activo
);
