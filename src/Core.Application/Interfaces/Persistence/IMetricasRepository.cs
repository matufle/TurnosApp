using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TurnosApp.Core.Application.DTOs.Metricas;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

// Repo puramente de agregación — no hay una entidad "Metrica", así que no hereda
// de IRepository<T>/GenericRepository<T>. Todos los métodos agregan en SQL sobre
// columnas mapeadas (nunca sobre las propiedades [NotMapped] de Cobro/Turno),
// salvo los dos casos documentados en MetricasRepository que materializan una
// proyección liviana y acotada por el rango de fechas antes de clasificar en memoria.
public interface IMetricasRepository
{
    Task<(decimal Ingresos, decimal Comision, decimal GananciaNeta, int Cantidad)> GetTotalesCobrosAsync(
        DateTime desde, DateTime hasta, int? metodoPagoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SerieMultipleDto>> GetIngresosComisionGananciaPorDiaAsync(
        DateTime desde, DateTime hasta, int? metodoPagoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RankingItemDto>> GetIngresosPorMetodoPagoAsync(
        DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DistribucionDto>> GetTurnosPorEstadoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RankingItemDto>> GetTopServiciosPorIngresosAsync(
        DateTime desde, DateTime hasta, int top, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RankingItemDto>> GetTopRecursosPorTurnosAsync(
        DateTime desde, DateTime hasta, int top, int? recursoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DistribucionDto>> GetEstadoPagoDistribucionAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default);

    Task<decimal> GetSaldoPendientePeriodoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HeatmapCeldaDto>> GetHeatmapTurnosAsync(
        DateTime desde, DateTime hasta, int? recursoId, EstadoTurno? estado, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PuntoSerieDto> Creados, IReadOnlyList<PuntoSerieDto> Completados)> GetEvolucionCreadosVsCompletadosAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RankingItemDto>> GetOcupacionPorRecursoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default);

    Task<decimal> GetAnticipacionPromedioHorasAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default);

    Task<(int Nuevos, int Recurrentes, int Inactivos, int TotalClientes)> GetResumenClientesAsync(
        DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PuntoSerieDto>> GetClientesNuevosPorMesAsync(
        DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RankingItemDto>> GetTopClientesPorFacturacionAsync(
        DateTime desde, DateTime hasta, int top, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<RankingItemDto> MasReservados, IReadOnlyList<RankingItemDto> MasRentables, IReadOnlyList<RankingItemDto> BajaDemanda)> GetServiciosRankingAsync(
        DateTime desde, DateTime hasta, int top, CancellationToken cancellationToken = default);

    // recursoId acota las 3 listas (incluida la de facturación): a diferencia del historial de
    // cobros (tenant-wide), esta es una comparación nominal ENTRE recursos/colegas — exactamente
    // lo que VerAgendaCompleta está pensado para restringir cuando el caller no lo tiene.
    Task<(IReadOnlyList<RankingItemDto> Facturacion, IReadOnlyList<RankingItemDto> Completados, IReadOnlyList<RankingItemDto> Cancelados)> GetRecursosFacturacionYEstadoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default);
}
