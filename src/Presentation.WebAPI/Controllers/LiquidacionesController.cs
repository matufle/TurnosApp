using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Liquidaciones;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/liquidaciones")]
[Produces("application/json")]
[Authorize]
public class LiquidacionesController : ControllerBase
{
    private readonly ILiquidacionAppService _liquidacionAppService;
    private readonly ILiquidacionGeneratorService _liquidacionGeneratorService;
    private readonly ICurrentUserService _currentUserService;

    public LiquidacionesController(
        ILiquidacionAppService liquidacionAppService,
        ILiquidacionGeneratorService liquidacionGeneratorService,
        ICurrentUserService currentUserService)
    {
        _liquidacionAppService = liquidacionAppService;
        _liquidacionGeneratorService = liquidacionGeneratorService;
        _currentUserService = currentUserService;
    }

    /// <summary>Todas las liquidaciones del tenant (todos los profesionales).</summary>
    [HttpGet]
    [RequierePermiso(Permiso.VerLiquidaciones, Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(IReadOnlyList<LiquidacionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var liquidaciones = await _liquidacionAppService.GetAllAsync(cancellationToken);
        return Ok(liquidaciones);
    }

    /// <summary>
    /// Self-service: liquidaciones del propio Recurso vinculado al usuario autenticado — sin
    /// permiso especial, mismo criterio que "mis turnos" (ownership vía Recurso.UsuarioId).
    /// </summary>
    [HttpGet("mias")]
    [ProducesResponseType(typeof(IReadOnlyList<LiquidacionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMias(CancellationToken cancellationToken)
    {
        var recursoId = await _currentUserService.GetCurrentRecursoIdAsync(cancellationToken);

        if (recursoId is null)
            return Ok(Array.Empty<LiquidacionListItemDto>());

        var liquidaciones = await _liquidacionAppService.GetByRecursoAsync(recursoId.Value, cancellationToken);
        return Ok(liquidaciones);
    }

    /// <summary>Detalle completo de una liquidación (líneas por turno + adelantos asignados).</summary>
    [HttpGet("{id:int}")]
    [RequierePermiso(Permiso.VerLiquidaciones, Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(LiquidacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var liquidacion = await _liquidacionAppService.GetByIdAsync(id, cancellationToken);
        return Ok(liquidacion);
    }

    /// <summary>Marca una liquidación Generada como Pagada.</summary>
    [HttpPatch("{id:int}/marcar-pagada")]
    [RequierePermiso(Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(LiquidacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarcarPagada(int id, [FromBody] MarcarPagadaLiquidacionDto dto, CancellationToken cancellationToken)
    {
        var liquidacion = await _liquidacionAppService.MarcarPagadaAsync(id, dto, cancellationToken);
        return Ok(liquidacion);
    }

    /// <summary>Anula una liquidación Generada (no Pagada) y libera sus turnos/adelantos para el próximo ciclo.</summary>
    [HttpPatch("{id:int}/anular")]
    [RequierePermiso(Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(LiquidacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Anular(int id, [FromBody] AnularLiquidacionDto dto, CancellationToken cancellationToken)
    {
        var liquidacion = await _liquidacionAppService.AnularAsync(id, dto, cancellationToken);
        return Ok(liquidacion);
    }

    /// <summary>
    /// Dispara la generación de liquidaciones pendientes para el tenant actual (el mismo
    /// proceso que corre automáticamente en LiquidacionGeneratorWorker) — útil para no
    /// esperar al próximo ciclo. Siempre opera sobre el tenant del caller, nunca uno arbitrario.
    /// </summary>
    [HttpPost("generar")]
    [RequierePermiso(Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Generar(CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.GetCurrentTenantId();
        await _liquidacionGeneratorService.GenerarParaTenantAsync(tenantId, cancellationToken);
        return NoContent();
    }
}
