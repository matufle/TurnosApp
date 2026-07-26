using System.Collections.Generic;

namespace TurnosApp.Core.Application.DTOs.Metricas;

public record TurnosMetricasDto(
    int TurnosTotales,
    decimal TasaCancelacion,
    decimal TasaAusentismo,
    decimal AnticipacionPromedioHoras,
    IReadOnlyList<HeatmapCeldaDto> Heatmap,
    IReadOnlyList<PuntoSerieDto> Creados,
    IReadOnlyList<PuntoSerieDto> Completados,
    IReadOnlyList<RankingItemDto> OcupacionPorRecurso
);
