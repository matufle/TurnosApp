using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TurnosApp.Core.Application.DTOs.Metricas;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class MetricasAppService : IMetricasAppService
{
    private const int TopN = 5;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    // Solo lectura/agregación: no crea ni muta ninguna TenantEntity, por lo que —a
    // diferencia de CobroAppService/TurnoAppService— no necesita ITenantProvider
    // (el filtro de tenant ya lo aplica el Global Query Filter en toda lectura).
    public MetricasAppService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ResumenMetricasDto> GetResumenAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var (desde, hasta) = ResolverRango(filtro);
        var recursoIdEfectivo = await ResolverRecursoScopeAsync(filtro.RecursoId, cancellationToken);

        var totalesCobros = await _unitOfWork.Metricas.GetTotalesCobrosAsync(desde, hasta, null, cancellationToken);
        var serieIngresos = await _unitOfWork.Metricas.GetIngresosComisionGananciaPorDiaAsync(desde, hasta, null, cancellationToken);
        var turnosPorEstado = await _unitOfWork.Metricas.GetTurnosPorEstadoAsync(desde, hasta, recursoIdEfectivo, cancellationToken);
        var saldoPendiente = await _unitOfWork.Metricas.GetSaldoPendientePeriodoAsync(desde, hasta, recursoIdEfectivo, cancellationToken);
        var topServicios = await _unitOfWork.Metricas.GetTopServiciosPorIngresosAsync(desde, hasta, TopN, cancellationToken);
        var topRecursos = await _unitOfWork.Metricas.GetTopRecursosPorTurnosAsync(desde, hasta, TopN, recursoIdEfectivo, cancellationToken);

        var totalTurnos = turnosPorEstado.Sum(d => d.Cantidad);
        var completados = CantidadPorEstado(turnosPorEstado, EstadoTurno.Completado);
        var cancelados = CantidadPorEstado(turnosPorEstado, EstadoTurno.Cancelado);

        return new ResumenMetricasDto(
            IngresosPeriodo: totalesCobros.Ingresos,
            TurnosCompletados: completados,
            TasaCancelacion: Porcentaje(cancelados, totalTurnos),
            SaldoPendientePeriodo: saldoPendiente,
            IngresosPorDia: serieIngresos.Select(s => new PuntoSerieDto(s.Etiqueta, s.Ingresos)).ToList(),
            TurnosPorEstado: turnosPorEstado,
            TopServiciosPorIngresos: topServicios,
            TopRecursosPorTurnos: topRecursos
        );
    }

    public async Task<IngresosMetricasDto> GetIngresosAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var (desde, hasta) = ResolverRango(filtro);
        var permisos = await _currentUserService.GetCurrentPermisosAsync(cancellationToken);
        var puedeVerGananciaNeta = permisos.HasFlag(Permiso.VerGananciaNeta);

        // El desglose de ingresos por método/comisión es tenant-wide (mismo criterio que
        // HistorialCobrosDto.TotalCobradoPeriodo, que tampoco se acota por Recurso). Solo la
        // distribución de EstadoPago depende de la agenda (turnos), así que respeta el scope.
        var recursoIdEfectivo = await ResolverRecursoScopeAsync(null, cancellationToken);

        var totales = await _unitOfWork.Metricas.GetTotalesCobrosAsync(desde, hasta, filtro.MetodoPagoId, cancellationToken);
        var serie = await _unitOfWork.Metricas.GetIngresosComisionGananciaPorDiaAsync(desde, hasta, filtro.MetodoPagoId, cancellationToken);
        var porMetodo = await _unitOfWork.Metricas.GetIngresosPorMetodoPagoAsync(desde, hasta, cancellationToken);
        var estadoPago = await _unitOfWork.Metricas.GetEstadoPagoDistribucionAsync(desde, hasta, recursoIdEfectivo, cancellationToken);

        return new IngresosMetricasDto(
            IngresosTotales: totales.Ingresos,
            GananciaNeta: puedeVerGananciaNeta ? totales.GananciaNeta : null,
            TicketPromedio: totales.Cantidad > 0 ? Math.Round(totales.Ingresos / totales.Cantidad, 2) : 0m,
            IngresosComisionGanancia: puedeVerGananciaNeta
                ? serie
                : serie.Select(s => new SerieMultipleDto(s.Etiqueta, s.Ingresos, null, null)).ToList(),
            IngresosPorMetodoPago: porMetodo,
            EstadoPagoTurnos: estadoPago
        );
    }

    public async Task<TurnosMetricasDto> GetTurnosAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var (desde, hasta) = ResolverRango(filtro);
        var recursoIdEfectivo = await ResolverRecursoScopeAsync(filtro.RecursoId, cancellationToken);
        var estado = ParsearEstado(filtro.Estado);

        var turnosPorEstado = await _unitOfWork.Metricas.GetTurnosPorEstadoAsync(desde, hasta, recursoIdEfectivo, cancellationToken);
        var heatmap = await _unitOfWork.Metricas.GetHeatmapTurnosAsync(desde, hasta, recursoIdEfectivo, estado, cancellationToken);
        var (creados, completadosSerie) = await _unitOfWork.Metricas.GetEvolucionCreadosVsCompletadosAsync(desde, hasta, recursoIdEfectivo, cancellationToken);
        var ocupacion = await _unitOfWork.Metricas.GetOcupacionPorRecursoAsync(desde, hasta, recursoIdEfectivo, cancellationToken);
        var anticipacion = await _unitOfWork.Metricas.GetAnticipacionPromedioHorasAsync(desde, hasta, recursoIdEfectivo, cancellationToken);

        var total = turnosPorEstado.Sum(d => d.Cantidad);
        var cancelados = CantidadPorEstado(turnosPorEstado, EstadoTurno.Cancelado);
        var ausentes = CantidadPorEstado(turnosPorEstado, EstadoTurno.Ausente);

        return new TurnosMetricasDto(
            TurnosTotales: total,
            TasaCancelacion: Porcentaje(cancelados, total),
            TasaAusentismo: Porcentaje(ausentes, total),
            AnticipacionPromedioHoras: anticipacion,
            Heatmap: heatmap,
            Creados: creados,
            Completados: completadosSerie,
            OcupacionPorRecurso: ocupacion
        );
    }

    public async Task<ClientesMetricasDto> GetClientesAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var (desde, hasta) = ResolverRango(filtro);

        var (nuevos, recurrentes, inactivos, totalClientes) =
            await _unitOfWork.Metricas.GetResumenClientesAsync(desde, hasta, cancellationToken);
        var nuevosPorMes = await _unitOfWork.Metricas.GetClientesNuevosPorMesAsync(desde, hasta, cancellationToken);
        var topClientes = await _unitOfWork.Metricas.GetTopClientesPorFacturacionAsync(desde, hasta, TopN, cancellationToken);

        var distribucion = new List<DistribucionDto>
        {
            new("Nuevos", nuevos, Porcentaje(nuevos, totalClientes)),
            new("Recurrentes", recurrentes, Porcentaje(recurrentes, totalClientes)),
            new("Inactivos", inactivos, Porcentaje(inactivos, totalClientes)),
        };

        return new ClientesMetricasDto(
            ClientesNuevos: nuevos,
            PorcentajeRecurrentes: Porcentaje(recurrentes, totalClientes),
            ClientesInactivos: inactivos,
            NuevosPorMes: nuevosPorMes,
            NuevosRecurrentesInactivos: distribucion,
            TopClientesPorFacturacion: topClientes
        );
    }

    public async Task<ServiciosRecursosMetricasDto> GetServiciosRecursosAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var (desde, hasta) = ResolverRango(filtro);
        var recursoIdEfectivo = await ResolverRecursoScopeAsync(null, cancellationToken);

        var (masReservados, masRentables, bajaDemanda) =
            await _unitOfWork.Metricas.GetServiciosRankingAsync(desde, hasta, TopN, cancellationToken);
        var (facturacion, completados, cancelados) =
            await _unitOfWork.Metricas.GetRecursosFacturacionYEstadoAsync(desde, hasta, recursoIdEfectivo, cancellationToken);

        return new ServiciosRecursosMetricasDto(
            ServiciosMasReservados: masReservados,
            ServiciosMasRentables: masRentables,
            ServiciosBajaDemanda: bajaDemanda,
            FacturacionPorRecurso: facturacion,
            CompletadosPorRecurso: completados,
            CanceladosPorRecurso: cancelados
        );
    }

    // Sin este permiso, el usuario queda acotado a los turnos de su propio Recurso vinculado
    // (mismo patrón que TurnoAppService.GetAllAsync) — se resuelve ANTES de llamar al repo para
    // que el Where entre en la misma query SQL, y se ignora cualquier recursoId pedido por
    // query string (evita escalar privilegios pidiendo el recurso de un colega).
    private async Task<int?> ResolverRecursoScopeAsync(int? recursoIdSolicitado, CancellationToken cancellationToken)
    {
        var permisos = await _currentUserService.GetCurrentPermisosAsync(cancellationToken);

        if (!permisos.HasFlag(Permiso.VerAgendaCompleta))
            return await _currentUserService.GetCurrentRecursoIdAsync(cancellationToken);

        return recursoIdSolicitado;
    }

    private static EstadoTurno? ParsearEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            return null;

        if (!Enum.TryParse<EstadoTurno>(estado, ignoreCase: true, out var resultado))
            throw new BadRequestException($"'{estado}' no es un estado de turno válido.");

        return resultado;
    }

    private static (DateTime Desde, DateTime Hasta) ResolverRango(MetricasFiltroDto filtro)
    {
        var hasta = filtro.FechaHasta ?? DateTime.UtcNow;
        var desde = filtro.FechaDesde ?? new DateTime(hasta.Year, hasta.Month, 1);

        if (desde > hasta)
            throw new BadRequestException("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");

        return (desde, hasta);
    }

    private static int CantidadPorEstado(IReadOnlyList<DistribucionDto> distribucion, EstadoTurno estado) =>
        distribucion.FirstOrDefault(d => d.Categoria == estado.ToString())?.Cantidad ?? 0;

    private static decimal Porcentaje(int parte, int total) =>
        total > 0 ? Math.Round(100m * parte / total, 1) : 0m;
}
