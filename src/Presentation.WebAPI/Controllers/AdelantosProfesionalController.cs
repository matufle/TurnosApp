using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.AdelantosProfesional;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/adelantosprofesional")]
[Produces("application/json")]
[Authorize]
public class AdelantosProfesionalController : ControllerBase
{
    private readonly IAdelantoProfesionalAppService _adelantoProfesionalAppService;

    public AdelantosProfesionalController(IAdelantoProfesionalAppService adelantoProfesionalAppService)
    {
        _adelantoProfesionalAppService = adelantoProfesionalAppService;
    }

    /// <summary>Adelantos/retiros cargados a un profesional, asignados o pendientes de asignar.</summary>
    [HttpGet]
    [RequierePermiso(Permiso.VerLiquidaciones, Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(IReadOnlyList<AdelantoProfesionalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRecurso([FromQuery] int recursoId, CancellationToken cancellationToken)
    {
        var adelantos = await _adelantoProfesionalAppService.GetByRecursoAsync(recursoId, cancellationToken);
        return Ok(adelantos);
    }

    /// <summary>
    /// Carga un adelanto. Si ya existe una liquidación Generada (no Pagada) cuyo período
    /// cubre la fecha, se asigna al vuelo; si no, lo recoge el worker en el próximo ciclo.
    /// </summary>
    [HttpPost]
    [RequierePermiso(Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(AdelantoProfesionalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateAdelantoProfesionalDto dto, CancellationToken cancellationToken)
    {
        var creado = await _adelantoProfesionalAppService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByRecurso), new { recursoId = creado.RecursoId }, creado);
    }
}
