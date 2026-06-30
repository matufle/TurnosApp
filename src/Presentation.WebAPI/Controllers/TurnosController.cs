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
    /// Reserva un nuevo turno para un cliente y recurso dados.
    /// Valida automáticamente el solapamiento con turnos existentes.
    /// Retorna 409 Conflict si el horario no está disponible.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TurnoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearTurno(
        [FromBody] CrearTurnoDto dto,
        CancellationToken cancellationToken)
    {
        // El GlobalExceptionHandler captura BusinessException (solapamiento)
        // y la traduce a 409 Conflict con ProblemDetails.
        // No necesitamos try/catch aquí — el pipeline lo maneja.
        var turno = await _turnoAppService.CrearTurnoAsync(dto, cancellationToken);

        // 201 Created: la respuesta incluye el header Location
        // apuntando al turno creado (cuando implementemos GetById de Turno).
        return StatusCode(StatusCodes.Status201Created, turno);
    }
}