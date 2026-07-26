using System.Collections.Generic;

namespace TurnosApp.Core.Application.DTOs.Metricas;

public record ResumenMetricasDto(
    decimal IngresosPeriodo,
    int TurnosCompletados,
    decimal TasaCancelacion,
    // Acotado al rango filtrado (turnos con FechaHoraInicio en rango) — a diferencia de
    // HistorialCobrosDto.SaldoPendienteGlobal, que es de todo el historial. Es intencional:
    // dentro de esta pestaña todos los KPIs son "de período".
    decimal SaldoPendientePeriodo,
    IReadOnlyList<PuntoSerieDto> IngresosPorDia,
    IReadOnlyList<DistribucionDto> TurnosPorEstado,
    IReadOnlyList<RankingItemDto> TopServiciosPorIngresos,
    IReadOnlyList<RankingItemDto> TopRecursosPorTurnos
);
