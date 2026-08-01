namespace TurnosApp.Core.Application.DTOs.Suscripcion;

public record SuscripcionDto(
    string EstadoSuscripcion,
    DateTime? SuscripcionVenceEn,
    bool EsGrandfathered,
    string? PlanNombre,
    decimal? PlanPrecioMensual,
    bool TieneSuscripcionIniciada
);
