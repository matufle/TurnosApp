using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Roles;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[RequierePermiso(Permiso.GestionarRoles)]
public class RolesController : ControllerBase
{
    private readonly IRolAppService _rolAppService;

    public RolesController(IRolAppService rolAppService)
    {
        _rolAppService = rolAppService;
    }

    /// <summary>Obtiene todos los roles del tenant actual.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _rolAppService.GetAllAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>Obtiene un rol por su ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var rol = await _rolAppService.GetByIdAsync(id, cancellationToken);
        return Ok(rol);
    }

    /// <summary>Crea un rol nuevo con los permisos elegidos.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRolDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _rolAppService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    /// <summary>Actualiza el nombre y los permisos de un rol existente (no admite el rol Admin).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateRolDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _rolAppService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Elimina un rol (no admite el rol Admin ni roles con usuarios asignados).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _rolAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
