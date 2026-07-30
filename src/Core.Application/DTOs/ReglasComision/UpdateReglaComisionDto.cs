namespace TurnosApp.Core.Application.DTOs.ReglasComision;

public record UpdateReglaComisionDto(
    string Tipo,
    decimal Valor,
    bool Activo
);
