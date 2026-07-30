using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Caja;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/caja")]
[Produces("application/json")]
[Authorize]
public class CajaController : ControllerBase
{
    private readonly ICajaAppService _cajaAppService;

    public CajaController(ICajaAppService cajaAppService)
    {
        _cajaAppService = cajaAppService;
    }

    /// <summary>Sesión de caja actualmente abierta para el tenant, o null si no hay ninguna.</summary>
    [HttpGet("sesion-abierta")]
    [RequierePermiso(Permiso.VerCaja, Permiso.GestionarCaja)]
    [ProducesResponseType(typeof(SesionCajaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSesionAbierta(CancellationToken cancellationToken)
    {
        var sesion = await _cajaAppService.GetSesionAbiertaAsync(cancellationToken);
        return Ok(sesion);
    }

    /// <summary>Detalle de una sesión de caja (abierta o cerrada), con movimientos y desglose por medio de pago.</summary>
    [HttpGet("sesiones/{id:int}")]
    [RequierePermiso(Permiso.VerCaja, Permiso.GestionarCaja)]
    [ProducesResponseType(typeof(SesionCajaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var sesion = await _cajaAppService.GetByIdAsync(id, cancellationToken);
        return Ok(sesion);
    }

    /// <summary>Listado paginado de sesiones de caja cerradas.</summary>
    [HttpGet("sesiones/historial")]
    [RequierePermiso(Permiso.VerCaja, Permiso.GestionarCaja)]
    [ProducesResponseType(typeof(HistorialSesionesCajaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var historial = await _cajaAppService.GetHistorialAsync(fechaDesde, fechaHasta, pagina, tamanoPagina, cancellationToken);
        return Ok(historial);
    }

    /// <summary>Abre una nueva sesión de caja. Falla si ya hay una abierta.</summary>
    [HttpPost("sesiones")]
    [RequierePermiso(Permiso.GestionarCaja)]
    [ProducesResponseType(typeof(SesionCajaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Abrir([FromBody] AbrirSesionCajaDto dto, CancellationToken cancellationToken)
    {
        var sesion = await _cajaAppService.AbrirSesionAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = sesion.Id }, sesion);
    }

    /// <summary>Registra un movimiento manual (ingreso/egreso) sobre la sesión de caja abierta.</summary>
    [HttpPost("movimientos")]
    [RequierePermiso(Permiso.GestionarCaja)]
    [ProducesResponseType(typeof(MovimientoCajaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoCajaDto dto, CancellationToken cancellationToken)
    {
        var movimiento = await _cajaAppService.RegistrarMovimientoAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, movimiento);
    }

    /// <summary>Cierra una sesión de caja. Cerrar una sesión abierta por otro usuario requiere el permiso ForzarCierreCaja.</summary>
    [HttpPatch("sesiones/{id:int}/cerrar")]
    [RequierePermiso(Permiso.GestionarCaja)]
    [ProducesResponseType(typeof(SesionCajaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cerrar(int id, [FromBody] CerrarSesionCajaDto dto, CancellationToken cancellationToken)
    {
        var sesion = await _cajaAppService.CerrarSesionAsync(id, dto, cancellationToken);
        return Ok(sesion);
    }
}
