using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Roles;

public record UpdateRolDto(
    string Nombre,
    IReadOnlyList<string> Permisos
);
