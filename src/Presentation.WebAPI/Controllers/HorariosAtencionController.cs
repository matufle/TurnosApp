using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Horarios;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/recursos/{recursoId:int}/horarios")]
[Produces("application/json")]
[Authorize]
public class HorariosAtencionController : ControllerBase
{
    private readonly IHorarioAtencionAppService _horarioAtencionAppService;

    public HorariosAtencionController(IHorarioAtencionAppService horarioAtencionAppService)
    {
        _horarioAtencionAppService = horarioAtencionAppService;
    }

    /// <summary>Horario de atención semanal del recurso. Sin gate: cualquier usuario del tenant puede consultarlo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HorarioAtencionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRecurso(int recursoId, CancellationToken cancellationToken)
    {
        var horarios = await _horarioAtencionAppService.GetByRecursoAsync(recursoId, cancellationToken);
        return Ok(horarios);
    }

    /// <summary>Reemplaza el horario semanal completo del recurso.</summary>
    [HttpPut]
    [RequierePermiso(Permiso.GestionarRecursos)]
    [ProducesResponseType(typeof(IReadOnlyList<HorarioAtencionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reemplazar(
        int recursoId,
        [FromBody] ReemplazarHorariosDto dto,
        CancellationToken cancellationToken)
    {
        var horarios = await _horarioAtencionAppService.ReemplazarAsync(recursoId, dto, cancellationToken);
        return Ok(horarios);
    }
}
