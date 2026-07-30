namespace TurnosApp.Core.Application.DTOs.ReglasComision;

public record CreateReglaComisionDto(
    int RecursoId,
    int? ServicioId,
    string Tipo,
    decimal Valor
);
