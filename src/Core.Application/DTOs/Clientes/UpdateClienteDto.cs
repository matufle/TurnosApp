using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Clientes;

public record UpdateClienteDto(
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? NotasAdicionales
);