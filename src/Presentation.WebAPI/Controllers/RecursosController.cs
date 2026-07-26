using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Recursos;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;
namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class RecursosController : ControllerBase
{
    private readonly IRecursoAppService _recursoAppService;

    public RecursosController(IRecursoAppService recursoAppService)
    {
        _recursoAppService = recursoAppService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecursoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var recursos = await _recursoAppService.GetAllAsync(cancellationToken);
        return Ok(recursos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RecursoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var recurso = await _recursoAppService.GetByIdAsync(id, cancellationToken);
        return Ok(recurso);
    }

    /// <summary>
    /// Usuarios que se pueden vincular a un Recurso (activos y sin otro Recurso ya asociado).
    /// Al editar, pasar recursoIdActual para que el usuario ya vinculado a ese mismo Recurso siga apareciendo.
    /// </summary>
    [HttpGet("usuarios-disponibles")]
    [RequierePermiso(Permiso.GestionarRecursos)]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioParaVincularDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsuariosDisponibles([FromQuery] int? recursoIdActual, CancellationToken cancellationToken)
    {
        var usuarios = await _recursoAppService.GetUsuariosDisponiblesAsync(recursoIdActual, cancellationToken);
        return Ok(usuarios);
    }

    [HttpPost]
    [RequierePermiso(Permiso.GestionarRecursos)]
    [ProducesResponseType(typeof(RecursoDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRecursoDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _recursoAppService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequierePermiso(Permiso.GestionarRecursos)]
    [ProducesResponseType(typeof(RecursoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateRecursoDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _recursoAppService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequierePermiso(Permiso.GestionarRecursos)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _recursoAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}