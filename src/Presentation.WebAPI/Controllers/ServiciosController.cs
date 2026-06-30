using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Servicios;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServiciosController : ControllerBase
{
    private readonly IServicioService _servicioService;

    public ServiciosController(IServicioService servicioService)
    {
        _servicioService = servicioService;
    }

    /// <summary>Obtiene todos los servicios del tenant actual.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServicioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var servicios = await _servicioService.GetAllAsync(cancellationToken);
        return Ok(servicios);
    }

    /// <summary>Obtiene un servicio por su ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServicioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var servicio = await _servicioService.GetByIdAsync(id, cancellationToken);
        return Ok(servicio);
    }

    /// <summary>Crea un nuevo servicio.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServicioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateServicioDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _servicioService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    /// <summary>Actualiza un servicio existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ServicioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateServicioDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _servicioService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Elimina un servicio.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _servicioService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
