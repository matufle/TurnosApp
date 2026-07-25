using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Usuarios;

public record UpdateUsuarioDto(
    string Nombre,
    int RolId
);
