namespace TurnosApp.Core.Application.DTOs.Caja;

public record AbrirSesionCajaDto(
    decimal MontoInicial,
    string? Observaciones
);
