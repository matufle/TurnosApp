namespace TurnosApp.Core.Application.DTOs.AdelantosProfesional;

public record CreateAdelantoProfesionalDto(
    int RecursoId,
    decimal Monto,
    DateTime Fecha,
    string? Concepto
);
