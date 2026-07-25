using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Usuarios;

/// <summary>
/// Alta de un usuario dentro del tenant del caller (no crea un Tenant nuevo,
/// a diferencia de RegisterRequestDTO). No hay invitación por mail: el Admin
/// entrega el email y la contraseña directamente.
/// </summary>
public record CreateUsuarioDto(
    string Nombre,
    string Email,
    string Password,
    int RolId
);
