using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Metricas;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[RequierePermiso(Permiso.VerReportes)]
public class MetricasController : ControllerBase
{
    private readonly IMetricasAppService _metricasAppService;

    public MetricasController(IMetricasAppService metricasAppService)
    {
        _metricasAppService = metricasAppService;
    }

    /// <summary>KPIs y gráficos de la pestaña Resumen (ingresos, turnos por estado, top servicios/recursos).</summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenMetricasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? recursoId,
        CancellationToken cancellationToken)
    {
        var filtro = new MetricasFiltroDto(fechaDesde, fechaHasta, recursoId, null, null, null);
        var resumen = await _metricasAppService.GetResumenAsync(filtro, cancellationToken);
        return Ok(resumen);
    }

    /// <summary>KPIs y gráficos de la pestaña Ingresos (ganancia neta gateada por VerGananciaNeta).</summary>
    [HttpGet("ingresos")]
    [ProducesResponseType(typeof(IngresosMetricasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIngresos(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? metodoPagoId,
        CancellationToken cancellationToken)
    {
        var filtro = new MetricasFiltroDto(fechaDesde, fechaHasta, null, null, metodoPagoId, null);
        var ingresos = await _metricasAppService.GetIngresosAsync(filtro, cancellationToken);
        return Ok(ingresos);
    }

    /// <summary>KPIs y gráficos de la pestaña Turnos (heatmap, ocupación, anticipación de reserva).</summary>
    [HttpGet("turnos")]
    [ProducesResponseType(typeof(TurnosMetricasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTurnos(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? recursoId,
        [FromQuery] string? estado,
        CancellationToken cancellationToken)
    {
        var filtro = new MetricasFiltroDto(fechaDesde, fechaHasta, recursoId, null, null, estado);
        var turnos = await _metricasAppService.GetTurnosAsync(filtro, cancellationToken);
        return Ok(turnos);
    }

    /// <summary>KPIs y gráficos de la pestaña Clientes (nuevos, recurrentes, inactivos, top facturación).</summary>
    [HttpGet("clientes")]
    [ProducesResponseType(typeof(ClientesMetricasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientes(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken cancellationToken)
    {
        var filtro = new MetricasFiltroDto(fechaDesde, fechaHasta, null, null, null, null);
        var clientes = await _metricasAppService.GetClientesAsync(filtro, cancellationToken);
        return Ok(clientes);
    }

    /// <summary>Rankings de la pestaña Servicios &amp; Recursos.</summary>
    [HttpGet("servicios-recursos")]
    [ProducesResponseType(typeof(ServiciosRecursosMetricasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiciosRecursos(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken cancellationToken)
    {
        var filtro = new MetricasFiltroDto(fechaDesde, fechaHasta, null, null, null, null);
        var serviciosRecursos = await _metricasAppService.GetServiciosRecursosAsync(filtro, cancellationToken);
        return Ok(serviciosRecursos);
    }
}
