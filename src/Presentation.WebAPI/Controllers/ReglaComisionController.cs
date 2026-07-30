using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.ReglasComision;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/reglascomision")]
[Produces("application/json")]
[Authorize]
public class ReglaComisionController : ControllerBase
{
    private readonly IReglaComisionAppService _reglaComisionAppService;

    public ReglaComisionController(IReglaComisionAppService reglaComisionAppService)
    {
        _reglaComisionAppService = reglaComisionAppService;
    }

    /// <summary>Reglas de comisión (base + overrides por servicio) de un profesional, incluye inactivas.</summary>
    [HttpGet]
    [RequierePermiso(Permiso.VerLiquidaciones, Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(IReadOnlyList<ReglaComisionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRecurso([FromQuery] int recursoId, CancellationToken cancellationToken)
    {
        var reglas = await _reglaComisionAppService.GetByRecursoAsync(recursoId, cancellationToken);
        return Ok(reglas);
    }

    /// <summary>Crea una regla de comisión (base si ServicioId es null, override puntual si tiene valor).</summary>
    [HttpPost]
    [RequierePermiso(Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(ReglaComisionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateReglaComisionDto dto, CancellationToken cancellationToken)
    {
        var creada = await _reglaComisionAppService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByRecurso), new { recursoId = creada.RecursoId }, creada);
    }

    /// <summary>Actualiza el tipo/valor/actividad de una regla de comisión existente.</summary>
    [HttpPut("{id:int}")]
    [RequierePermiso(Permiso.GestionarLiquidaciones)]
    [ProducesResponseType(typeof(ReglaComisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReglaComisionDto dto, CancellationToken cancellationToken)
    {
        var actualizada = await _reglaComisionAppService.UpdateAsync(id, dto, cancellationToken);
        return Ok(actualizada);
    }
}
