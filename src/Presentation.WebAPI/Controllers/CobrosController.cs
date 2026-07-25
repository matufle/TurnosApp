using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Cobros;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CobrosController : ControllerBase
{
    private readonly ICobroAppService _cobroAppService;

    public CobrosController(ICobroAppService cobroAppService)
    {
        _cobroAppService = cobroAppService;
    }

    /// <summary>Lista los cobros de un turno. El parámetro turnoId es obligatorio.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CobroDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll([FromQuery] int? turnoId, CancellationToken cancellationToken)
    {
        if (turnoId is null)
            throw new BadRequestException("El parámetro 'turnoId' es obligatorio.");

        var cobros = await _cobroAppService.GetAllByTurnoIdAsync(turnoId.Value, cancellationToken);
        return Ok(cobros);
    }

    /// <summary>Obtiene un cobro por su ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CobroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var cobro = await _cobroAppService.GetByIdAsync(id, cancellationToken);
        return Ok(cobro);
    }

    /// <summary>Registra un cobro (total o parcial) sobre un turno.</summary>
    [HttpPost]
    [RequierePermiso(Permiso.CrearCobros)]
    [ProducesResponseType(typeof(CobroDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCobroDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _cobroAppService.CrearCobroAsync(dto, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    /// <summary>Edita un cobro existente (re-toma el % vigente del método seleccionado).</summary>
    [HttpPut("{id:int}")]
    [RequierePermiso(Permiso.CrearCobros)]
    [ProducesResponseType(typeof(CobroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCobroDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _cobroAppService.ActualizarCobroAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Listado paginado de cobros del tenant, filtrable por rango de fechas y búsqueda de cliente/turno.</summary>
    [HttpGet("historial")]
    [ProducesResponseType(typeof(HistorialCobrosDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? busqueda,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var historial = await _cobroAppService.GetHistorialAsync(
            fechaDesde, fechaHasta, busqueda, pagina, tamanoPagina, cancellationToken);

        return Ok(historial);
    }
}
