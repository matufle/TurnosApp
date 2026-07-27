using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Turnos;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

/// <summary>
/// Recurso propio del cliente autenticado (self-service) — no es un permiso de staff,
/// por eso no lleva [RequierePermiso], solo [Authorize] + el claim ClienteId del token.
/// </summary>
[ApiController]
[Route("api/mis-turnos")]
[Authorize]
public class MisTurnosController : ControllerBase
{
    private readonly ITurnoAppService _turnoAppService;
    private readonly ICurrentClienteService _currentClienteService;

    public MisTurnosController(ITurnoAppService turnoAppService, ICurrentClienteService currentClienteService)
    {
        _turnoAppService = turnoAppService;
        _currentClienteService = currentClienteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TurnoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisTurnos(CancellationToken cancellationToken)
    {
        var clienteId = _currentClienteService.GetCurrentClienteId();
        var turnos = await _turnoAppService.GetMisTurnosAsync(clienteId, cancellationToken);
        return Ok(turnos);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TurnoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] CrearTurnoPublicoDto dto, CancellationToken cancellationToken)
    {
        var clienteId = _currentClienteService.GetCurrentClienteId();
        var turno = await _turnoAppService.CrearTurnoPublicoAsync(clienteId, dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, turno);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
    {
        var clienteId = _currentClienteService.GetCurrentClienteId();
        await _turnoAppService.CancelarPropioAsync(clienteId, id, cancellationToken);
        return NoContent();
    }
}
