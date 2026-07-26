using System.Collections.Generic;

namespace TurnosApp.Core.Application.DTOs.Metricas;

public record ServiciosRecursosMetricasDto(
    IReadOnlyList<RankingItemDto> ServiciosMasReservados,
    IReadOnlyList<RankingItemDto> ServiciosMasRentables,
    IReadOnlyList<RankingItemDto> ServiciosBajaDemanda,
    IReadOnlyList<RankingItemDto> FacturacionPorRecurso,
    IReadOnlyList<RankingItemDto> CompletadosPorRecurso,
    IReadOnlyList<RankingItemDto> CanceladosPorRecurso
);
