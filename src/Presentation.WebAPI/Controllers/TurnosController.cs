using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Turnos;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TurnosController : ControllerBase
{
    private readonly ITurnoAppService _turnoAppService;

    public TurnosController(ITurnoAppService turnoAppService)
    {
        _turnoAppService = turnoAppService;
    }

    /// <summary>
    /// Lista todos los turnos activos del tenant actual (excluye cancelados).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TurnoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var turnos = await _turnoAppService.GetAllAsync(cancellationToken);
        return Ok(turnos);
    }

    /// <summary>
    /// Reserva un nuevo turno validando solapamiento.
    /// Retorna 409 si el horario no está disponible para el recurso.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TurnoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearTurno(
        [FromBody] CrearTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var turno = await _turnoAppService.CrearTurnoAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, turno);
    }

    /// <summary>
    /// Soft Delete: cancela el turno cambiando su estado a Cancelado.
    /// El registro se conserva en la base de datos para auditoría.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelarTurno(int id, CancellationToken cancellationToken)
    {
        await _turnoAppService.CancelarTurnoAsync(id, cancellationToken);
        return NoContent();
    }
}