namespace TurnosApp.Core.Application.DTOs.ReglasComision;

public record ReglaComisionDto(
    int Id,
    int RecursoId,
    int? ServicioId,
    string? ServicioNombre,
    string Tipo,
    decimal Valor,
    bool Activo
);
