namespace TurnosApp.Core.Application.DTOs.AdelantosProfesional;

public record AdelantoProfesionalDto(
    int Id,
    int RecursoId,
    decimal Monto,
    DateTime Fecha,
    string? Concepto,
    int? LiquidacionId
);
