namespace TurnosApp.Core.Application.DTOs.Metricas;

// Punto de la serie temporal apilada de Ingresos (Ingresos vs Comisión vs Ganancia Neta).
public record SerieMultipleDto(
    string Etiqueta,
    decimal Ingresos,
    // null cuando el caller no tiene el permiso VerGananciaNeta.
    decimal? Comision,
    decimal? GananciaNeta
);
