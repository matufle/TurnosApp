namespace TurnosApp.Core.Application.DTOs.Caja;

public record CerrarSesionCajaDto(
    decimal MontoFinalDeclarado,
    string? Observaciones
);
