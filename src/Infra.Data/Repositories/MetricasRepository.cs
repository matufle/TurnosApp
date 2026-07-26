using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.DTOs.Metricas;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

// No hereda de GenericRepository<T>: no hay una entidad "Metrica", todos los métodos
// son agregación custom sobre Turno/Cobro/TurnoServicio ya existentes.
//
// Principio general: el Where de rango de fechas (y de scope por recurso, cuando
// falta VerAgendaCompleta) se aplica ANTES de cualquier GroupBy/Sum, para que EF Core
// genere un único SELECT ... WHERE ... GROUP BY. La fórmula de Cobro.PrecioFinal/
// MontoComision/GananciaNeta ([NotMapped] en la entidad, ver Cobro.cs) se reescribe
// inline sobre las columnas SÍ mapeadas (PrecioBase, TipoModificadorSnapshot,
// PorcentajeModificadorSnapshot, PorcentajeComisionSnapshot) para que el Sum/GroupBy
// corra en SQL — nunca se materializa el historial completo como hace
// CobroRepository.GetHistorialAsync.
public class MetricasRepository : IMetricasRepository
{
    private readonly ApplicationDbContext _context;

    public MetricasRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(decimal Ingresos, decimal Comision, decimal GananciaNeta, int Cantidad)> GetTotalesCobrosAsync(
        DateTime desde, DateTime hasta, int? metodoPagoId, CancellationToken cancellationToken = default)
    {
        var query = _context.Cobros.AsNoTracking()
            .Where(c => c.CreadoEn >= desde && c.CreadoEn <= hasta);

        if (metodoPagoId.HasValue)
            query = query.Where(c => c.MetodoPagoId == metodoPagoId.Value);

        var resultado = await query
            .Select(c => new
            {
                PrecioFinal = c.PrecioBase + (
                    c.TipoModificadorSnapshot == TipoModificadorPago.Bonificacion
                        ? -(c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m)
                        : c.TipoModificadorSnapshot == TipoModificadorPago.Recargo
                            ? c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m
                            : 0m),
                c.PorcentajeComisionSnapshot
            })
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Ingresos = g.Sum(x => x.PrecioFinal),
                Comision = g.Sum(x => x.PrecioFinal * x.PorcentajeComisionSnapshot / 100m),
                Cantidad = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (resultado is null)
            return (0m, 0m, 0m, 0);

        return (resultado.Ingresos, resultado.Comision, resultado.Ingresos - resultado.Comision, resultado.Cantidad);
    }

    public async Task<IReadOnlyList<SerieMultipleDto>> GetIngresosComisionGananciaPorDiaAsync(
        DateTime desde, DateTime hasta, int? metodoPagoId, CancellationToken cancellationToken = default)
    {
        var query = _context.Cobros.AsNoTracking()
            .Where(c => c.CreadoEn >= desde && c.CreadoEn <= hasta);

        if (metodoPagoId.HasValue)
            query = query.Where(c => c.MetodoPagoId == metodoPagoId.Value);

        var porDia = await query
            .GroupBy(c => c.CreadoEn.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Ingresos = g.Sum(c => c.PrecioBase + (
                    c.TipoModificadorSnapshot == TipoModificadorPago.Bonificacion
                        ? -(c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m)
                        : c.TipoModificadorSnapshot == TipoModificadorPago.Recargo
                            ? c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m
                            : 0m)),
                Comision = g.Sum(c =>
                    (c.PrecioBase + (
                        c.TipoModificadorSnapshot == TipoModificadorPago.Bonificacion
                            ? -(c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m)
                            : c.TipoModificadorSnapshot == TipoModificadorPago.Recargo
                                ? c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m
                                : 0m)) * c.PorcentajeComisionSnapshot / 100m)
            })
            .OrderBy(x => x.Fecha)
            .ToListAsync(cancellationToken);

        return porDia
            .Select(x => new SerieMultipleDto(
                Etiqueta: x.Fecha.ToString("yyyy-MM-dd"),
                Ingresos: x.Ingresos,
                Comision: x.Comision,
                GananciaNeta: x.Ingresos - x.Comision))
            .ToList();
    }

    public async Task<IReadOnlyList<RankingItemDto>> GetIngresosPorMetodoPagoAsync(
        DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        // Se agrupa por el snapshot del nombre, no por MetodoPagoId, para que un método
        // de pago ya desactivado/borrado siga apareciendo en el histórico agregado.
        var porMetodo = await _context.Cobros.AsNoTracking()
            .Where(c => c.CreadoEn >= desde && c.CreadoEn <= hasta)
            .GroupBy(c => c.NombreMetodoPagoSnapshot)
            .Select(g => new
            {
                Nombre = g.Key,
                Ingresos = g.Sum(c => c.PrecioBase + (
                    c.TipoModificadorSnapshot == TipoModificadorPago.Bonificacion
                        ? -(c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m)
                        : c.TipoModificadorSnapshot == TipoModificadorPago.Recargo
                            ? c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m
                            : 0m)),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Ingresos)
            .ToListAsync(cancellationToken);

        return porMetodo
            .Select(x => new RankingItemDto(Id: 0, Nombre: x.Nombre, Valor: x.Ingresos, Cantidad: x.Cantidad))
            .ToList();
    }

    public async Task<IReadOnlyList<DistribucionDto>> GetTurnosPorEstadoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default)
    {
        var query = _context.Turnos.AsNoTracking()
            .Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio <= hasta);

        if (recursoId.HasValue)
            query = query.Where(t => t.RecursoId == recursoId.Value);

        var porEstado = await query
            .GroupBy(t => t.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        var total = porEstado.Sum(x => x.Cantidad);

        return porEstado
            .Select(x => new DistribucionDto(
                Categoria: x.Estado.ToString(),
                Cantidad: x.Cantidad,
                Porcentaje: total > 0 ? Math.Round(100m * x.Cantidad / total, 1) : 0m))
            .ToList();
    }

    public async Task<IReadOnlyList<RankingItemDto>> GetTopServiciosPorIngresosAsync(
        DateTime desde, DateTime hasta, int top, CancellationToken cancellationToken = default)
    {
        var porServicio = await _context.TurnoServicios.AsNoTracking()
            .Where(ts => ts.Turno.FechaHoraInicio >= desde && ts.Turno.FechaHoraInicio <= hasta)
            .GroupBy(ts => new { ts.ServicioId, ts.Servicio.Nombre })
            .Select(g => new
            {
                g.Key.ServicioId,
                g.Key.Nombre,
                Ingresos = g.Sum(ts => ts.PrecioAplicado),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Ingresos)
            .Take(top)
            .ToListAsync(cancellationToken);

        return porServicio
            .Select(x => new RankingItemDto(x.ServicioId, x.Nombre, x.Ingresos, x.Cantidad))
            .ToList();
    }

    public async Task<IReadOnlyList<RankingItemDto>> GetTopRecursosPorTurnosAsync(
        DateTime desde, DateTime hasta, int top, int? recursoId, CancellationToken cancellationToken = default)
    {
        var query = _context.Turnos.AsNoTracking()
            .Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio <= hasta);

        if (recursoId.HasValue)
            query = query.Where(t => t.RecursoId == recursoId.Value);

        var porRecurso = await query
            .GroupBy(t => new { t.RecursoId, t.Recurso.Nombre })
            .Select(g => new { g.Key.RecursoId, g.Key.Nombre, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .Take(top)
            .ToListAsync(cancellationToken);

        return porRecurso
            .Select(x => new RankingItemDto(x.RecursoId, x.Nombre, x.Cantidad, x.Cantidad))
            .ToList();
    }

    // Proyección liviana compartida (2 decimales por turno del rango filtrado, no el grafo
    // completo de entidades) — necesaria porque EstadoPago/SaldoPendiente se clasifican por
    // turno individual y no son agregables directamente en un solo GROUP BY. El volumen está
    // acotado por el filtro de fecha (y de recurso), no por todo el historial.
    private async Task<List<(decimal PrecioTotal, decimal MontoCobrado)>> ObtenerSaldosTurnosAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken)
    {
        var query = _context.Turnos.AsNoTracking()
            .Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio <= hasta)
            .Where(t => t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Ausente);

        if (recursoId.HasValue)
            query = query.Where(t => t.RecursoId == recursoId.Value);

        var raw = await query
            .Select(t => new
            {
                PrecioTotal = t.TurnoServicios.Sum(ts => ts.PrecioAplicado),
                MontoCobrado = t.Cobros.Sum(c => c.PrecioBase)
            })
            .ToListAsync(cancellationToken);

        return raw.Select(x => (x.PrecioTotal, x.MontoCobrado)).ToList();
    }

    public async Task<IReadOnlyList<DistribucionDto>> GetEstadoPagoDistribucionAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default)
    {
        var saldos = await ObtenerSaldosTurnosAsync(desde, hasta, recursoId, cancellationToken);

        int sinCobrar = 0, parcial = 0, pagado = 0;
        foreach (var (precioTotal, montoCobrado) in saldos)
        {
            if (montoCobrado <= 0) sinCobrar++;
            else if (precioTotal - montoCobrado <= 0) pagado++;
            else parcial++;
        }

        var total = saldos.Count;
        return new List<DistribucionDto>
        {
            new("Pagado", pagado, total > 0 ? Math.Round(100m * pagado / total, 1) : 0m),
            new("Parcial", parcial, total > 0 ? Math.Round(100m * parcial / total, 1) : 0m),
            new("SinCobrar", sinCobrar, total > 0 ? Math.Round(100m * sinCobrar / total, 1) : 0m),
        };
    }

    public async Task<decimal> GetSaldoPendientePeriodoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default)
    {
        var saldos = await ObtenerSaldosTurnosAsync(desde, hasta, recursoId, cancellationToken);
        return saldos.Sum(x => x.PrecioTotal - x.MontoCobrado);
    }

    public async Task<IReadOnlyList<HeatmapCeldaDto>> GetHeatmapTurnosAsync(
        DateTime desde, DateTime hasta, int? recursoId, EstadoTurno? estado, CancellationToken cancellationToken = default)
    {
        var query = _context.Turnos.AsNoTracking()
            .Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio <= hasta);

        if (recursoId.HasValue)
            query = query.Where(t => t.RecursoId == recursoId.Value);

        if (estado.HasValue)
            query = query.Where(t => t.Estado == estado.Value);

        // Npgsql traduce DayOfWeek/Hour vía EXTRACT(...). Si en algún entorno no tradujera,
        // el fallback es proyectar {FechaHoraInicio} del rango (acotado) y agrupar en memoria.
        var celdas = await query
            .GroupBy(t => new { t.FechaHoraInicio.DayOfWeek, t.FechaHoraInicio.Hour })
            .Select(g => new { g.Key.DayOfWeek, g.Key.Hour, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        return celdas
            .Select(x => new HeatmapCeldaDto((int)x.DayOfWeek, x.Hour, x.Cantidad))
            .ToList();
    }

    public async Task<(IReadOnlyList<PuntoSerieDto> Creados, IReadOnlyList<PuntoSerieDto> Completados)> GetEvolucionCreadosVsCompletadosAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default)
    {
        var creadosQuery = _context.Turnos.AsNoTracking()
            .Where(t => t.CreadoEn >= desde && t.CreadoEn <= hasta);
        if (recursoId.HasValue) creadosQuery = creadosQuery.Where(t => t.RecursoId == recursoId.Value);

        var creados = await creadosQuery
            .GroupBy(t => t.CreadoEn.Date)
            .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
            .OrderBy(x => x.Fecha)
            .ToListAsync(cancellationToken);

        var completadosQuery = _context.Turnos.AsNoTracking()
            .Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio <= hasta && t.Estado == EstadoTurno.Completado);
        if (recursoId.HasValue) completadosQuery = completadosQuery.Where(t => t.RecursoId == recursoId.Value);

        var completados = await completadosQuery
            .GroupBy(t => t.FechaHoraInicio.Date)
            .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
            .OrderBy(x => x.Fecha)
            .ToListAsync(cancellationToken);

        return (
            creados.Select(x => new PuntoSerieDto(x.Fecha.ToString("yyyy-MM-dd"), x.Cantidad)).ToList(),
            completados.Select(x => new PuntoSerieDto(x.Fecha.ToString("yyyy-MM-dd"), x.Cantidad)).ToList()
        );
    }

    public async Task<IReadOnlyList<RankingItemDto>> GetOcupacionPorRecursoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default)
    {
        var query = _context.TurnoServicios.AsNoTracking()
            .Where(ts => ts.Turno.FechaHoraInicio >= desde && ts.Turno.FechaHoraInicio <= hasta);

        if (recursoId.HasValue)
            query = query.Where(ts => ts.Turno.RecursoId == recursoId.Value);

        var porRecurso = await query
            .GroupBy(ts => new { ts.Turno.RecursoId, ts.Turno.Recurso.Nombre })
            .Select(g => new
            {
                g.Key.RecursoId,
                g.Key.Nombre,
                MinutosOcupados = g.Sum(ts => ts.Servicio.DuracionMinutos),
                // Cantidad de líneas de servicio prestadas (no turnos distintos) — evita un
                // Distinct().Count() anidado dentro del GroupBy, de traducción menos confiable.
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.MinutosOcupados)
            .ToListAsync(cancellationToken);

        return porRecurso
            .Select(x => new RankingItemDto(
                Id: x.RecursoId,
                Nombre: x.Nombre,
                Valor: Math.Round(x.MinutosOcupados / 60m, 1),
                Cantidad: x.Cantidad))
            .ToList();
    }

    public async Task<decimal> GetAnticipacionPromedioHorasAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default)
    {
        var query = _context.Turnos.AsNoTracking()
            .Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio <= hasta);

        if (recursoId.HasValue)
            query = query.Where(t => t.RecursoId == recursoId.Value);

        // (FechaHoraInicio - CreadoEn) como TimeSpan no siempre traduce de forma confiable
        // para Average vía Npgsql: proyectamos las 2 columnas (acotado al rango) y promediamos en memoria.
        var fechas = await query
            .Select(t => new { t.FechaHoraInicio, t.CreadoEn })
            .ToListAsync(cancellationToken);

        if (fechas.Count == 0)
            return 0m;

        var promedioHoras = fechas.Average(x => (x.FechaHoraInicio - x.CreadoEn).TotalHours);
        return Math.Round((decimal)promedioHoras, 1);
    }

    public async Task<(int Nuevos, int Recurrentes, int Inactivos, int TotalClientes)> GetResumenClientesAsync(
        DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        // Cliente no tiene fecha de alta propia (ver Core.Domain.Entities.Cliente):
        // MIN(FechaHoraInicio) por cliente es el proxy de "alta" y MAX la última actividad.
        // GroupBy+Min/Max/Count se traduce a SQL; se materializa 1 fila por cliente CON
        // turnos (acotado por cantidad de clientes, no por cantidad de turnos).
        var porCliente = await _context.Turnos.AsNoTracking()
            .GroupBy(t => t.ClienteId)
            .Select(g => new
            {
                PrimerTurno = g.Min(t => t.FechaHoraInicio),
                UltimoTurno = g.Max(t => t.FechaHoraInicio),
                Cantidad = g.Count()
            })
            .ToListAsync(cancellationToken);

        var umbralInactividad = DateTime.UtcNow.AddDays(-60);

        var nuevos = porCliente.Count(c => c.PrimerTurno >= desde && c.PrimerTurno <= hasta);
        var recurrentes = porCliente.Count(c => c.Cantidad > 1);
        var inactivos = porCliente.Count(c => c.UltimoTurno < umbralInactividad);

        return (nuevos, recurrentes, inactivos, porCliente.Count);
    }

    public async Task<IReadOnlyList<PuntoSerieDto>> GetClientesNuevosPorMesAsync(
        DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        var primerTurnoPorCliente = _context.Turnos.AsNoTracking()
            .GroupBy(t => t.ClienteId)
            .Select(g => g.Min(t => t.FechaHoraInicio));

        // Subquery sobre una proyección ya agregada (Min por cliente) — a validar en runtime
        // que Npgsql traduzca el GroupBy anidado; si no, el fallback es materializar
        // primerTurnoPorCliente (acotado por cantidad de clientes) y agrupar por mes en memoria.
        var porMes = await primerTurnoPorCliente
            .Where(f => f >= desde && f <= hasta)
            .GroupBy(f => new { f.Year, f.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Cantidad = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        return porMes
            .Select(x => new PuntoSerieDto($"{x.Year:D4}-{x.Month:D2}", x.Cantidad))
            .ToList();
    }

    public async Task<IReadOnlyList<RankingItemDto>> GetTopClientesPorFacturacionAsync(
        DateTime desde, DateTime hasta, int top, CancellationToken cancellationToken = default)
    {
        var porCliente = await _context.Cobros.AsNoTracking()
            .Where(c => c.CreadoEn >= desde && c.CreadoEn <= hasta)
            .GroupBy(c => new { c.Turno.ClienteId, c.Turno.Cliente.Nombre, c.Turno.Cliente.Apellido })
            .Select(g => new
            {
                g.Key.ClienteId,
                NombreCompleto = g.Key.Nombre + " " + g.Key.Apellido,
                Facturado = g.Sum(c => c.PrecioBase + (
                    c.TipoModificadorSnapshot == TipoModificadorPago.Bonificacion
                        ? -(c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m)
                        : c.TipoModificadorSnapshot == TipoModificadorPago.Recargo
                            ? c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m
                            : 0m)),
                // Cantidad de cobros (no turnos distintos) — mismo criterio que en GetOcupacionPorRecursoAsync.
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Facturado)
            .Take(top)
            .ToListAsync(cancellationToken);

        return porCliente
            .Select(x => new RankingItemDto(x.ClienteId, x.NombreCompleto, x.Facturado, x.Cantidad))
            .ToList();
    }

    public async Task<(IReadOnlyList<RankingItemDto> MasReservados, IReadOnlyList<RankingItemDto> MasRentables, IReadOnlyList<RankingItemDto> BajaDemanda)> GetServiciosRankingAsync(
        DateTime desde, DateTime hasta, int top, CancellationToken cancellationToken = default)
    {
        // Universo acotado: servicios ACTIVOS del tenant con turnos en el rango — se agrega
        // una sola vez en SQL y los 3 rankings se derivan reordenando en memoria un resultado ya chico.
        var porServicio = await _context.TurnoServicios.AsNoTracking()
            .Where(ts => ts.Turno.FechaHoraInicio >= desde && ts.Turno.FechaHoraInicio <= hasta && ts.Servicio.Activo)
            .GroupBy(ts => new { ts.ServicioId, ts.Servicio.Nombre })
            .Select(g => new
            {
                g.Key.ServicioId,
                g.Key.Nombre,
                Reservas = g.Count(),
                Ingresos = g.Sum(ts => ts.PrecioAplicado)
            })
            .ToListAsync(cancellationToken);

        var masReservados = porServicio
            .OrderByDescending(x => x.Reservas).Take(top)
            .Select(x => new RankingItemDto(x.ServicioId, x.Nombre, x.Ingresos, x.Reservas))
            .ToList();

        var masRentables = porServicio
            .OrderByDescending(x => x.Ingresos).Take(top)
            .Select(x => new RankingItemDto(x.ServicioId, x.Nombre, x.Ingresos, x.Reservas))
            .ToList();

        var bajaDemanda = porServicio
            .OrderBy(x => x.Reservas).Take(top)
            .Select(x => new RankingItemDto(x.ServicioId, x.Nombre, x.Ingresos, x.Reservas))
            .ToList();

        return (masReservados, masRentables, bajaDemanda);
    }

    public async Task<(IReadOnlyList<RankingItemDto> Facturacion, IReadOnlyList<RankingItemDto> Completados, IReadOnlyList<RankingItemDto> Cancelados)> GetRecursosFacturacionYEstadoAsync(
        DateTime desde, DateTime hasta, int? recursoId, CancellationToken cancellationToken = default)
    {
        var facturacionQuery = _context.Cobros.AsNoTracking()
            .Where(c => c.CreadoEn >= desde && c.CreadoEn <= hasta);

        if (recursoId.HasValue)
            facturacionQuery = facturacionQuery.Where(c => c.Turno.RecursoId == recursoId.Value);

        var facturacion = await facturacionQuery
            .GroupBy(c => new { c.Turno.RecursoId, c.Turno.Recurso.Nombre })
            .Select(g => new
            {
                g.Key.RecursoId,
                g.Key.Nombre,
                Facturado = g.Sum(c => c.PrecioBase + (
                    c.TipoModificadorSnapshot == TipoModificadorPago.Bonificacion
                        ? -(c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m)
                        : c.TipoModificadorSnapshot == TipoModificadorPago.Recargo
                            ? c.PrecioBase * c.PorcentajeModificadorSnapshot / 100m
                            : 0m)),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Facturado)
            .ToListAsync(cancellationToken);

        var turnosQuery = _context.Turnos.AsNoTracking()
            .Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio <= hasta);

        if (recursoId.HasValue)
            turnosQuery = turnosQuery.Where(t => t.RecursoId == recursoId.Value);

        var completados = await turnosQuery
            .Where(t => t.Estado == EstadoTurno.Completado)
            .GroupBy(t => new { t.RecursoId, t.Recurso.Nombre })
            .Select(g => new { g.Key.RecursoId, g.Key.Nombre, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        var cancelados = await turnosQuery
            .Where(t => t.Estado == EstadoTurno.Cancelado)
            .GroupBy(t => new { t.RecursoId, t.Recurso.Nombre })
            .Select(g => new { g.Key.RecursoId, g.Key.Nombre, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        return (
            facturacion.Select(x => new RankingItemDto(x.RecursoId, x.Nombre, x.Facturado, x.Cantidad)).ToList(),
            completados.Select(x => new RankingItemDto(x.RecursoId, x.Nombre, x.Cantidad, x.Cantidad)).ToList(),
            cancelados.Select(x => new RankingItemDto(x.RecursoId, x.Nombre, x.Cantidad, x.Cantidad)).ToList()
        );
    }
}
