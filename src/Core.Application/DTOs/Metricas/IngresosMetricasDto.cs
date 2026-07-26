using System.Collections.Generic;

namespace TurnosApp.Core.Application.DTOs.Metricas;

public record IngresosMetricasDto(
    decimal IngresosTotales,
    // null cuando el caller no tiene el permiso VerGananciaNeta.
    decimal? GananciaNeta,
    decimal TicketPromedio,
    IReadOnlyList<SerieMultipleDto> IngresosComisionGanancia,
    IReadOnlyList<RankingItemDto> IngresosPorMetodoPago,
    IReadOnlyList<DistribucionDto> EstadoPagoTurnos
);
