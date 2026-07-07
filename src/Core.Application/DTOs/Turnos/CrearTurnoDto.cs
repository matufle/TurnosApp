using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.DTOs.Turnos;

// TurnosApp.Core.Application/DTOs/Turnos/CrearTurnoDto.cs
public record CrearTurnoDto(
    int? ClienteId,                    // ahora nullable
    ClienteInlineDto? ClienteNuevo,    // nuevo
    int RecursoId,
    IReadOnlyList<int> ServicioIds,
    DateTime FechaHoraInicio);

public record ClienteInlineDto(string Nombre, string Apellido, string? Telefono);