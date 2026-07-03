using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Recursos;
using TurnosApp.Core.Application.Interfaces.Services;
namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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

    [HttpPost]
    [ProducesResponseType(typeof(RecursoDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRecursoDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _recursoAppService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _recursoAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}