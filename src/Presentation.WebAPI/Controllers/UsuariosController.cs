using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Usuarios;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[RequierePermiso(Permiso.GestionarUsuarios)]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioAppService _usuarioAppService;

    public UsuariosController(IUsuarioAppService usuarioAppService)
    {
        _usuarioAppService = usuarioAppService;
    }

    /// <summary>Obtiene todos los usuarios del tenant actual (incluye inactivos).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioAppService.GetAllAsync(cancellationToken);
        return Ok(usuarios);
    }

    /// <summary>Obtiene un usuario por su ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioAppService.GetByIdAsync(id, cancellationToken);
        return Ok(usuario);
    }

    /// <summary>
    /// Crea un usuario dentro del tenant actual (no crea un Tenant nuevo). No hay invitación
    /// por mail: el email y la contraseña se entregan directamente acá.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _usuarioAppService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    /// <summary>Actualiza el nombre y el rol de un usuario existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _usuarioAppService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Reactiva un usuario previamente desactivado.</summary>
    [HttpPatch("{id:int}/activar")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id, CancellationToken cancellationToken)
    {
        var activado = await _usuarioAppService.ActivarAsync(id, cancellationToken);
        return Ok(activado);
    }

    /// <summary>
    /// Baja lógica: nunca se expone un DELETE físico de Usuario. Un usuario no puede
    /// desactivarse a sí mismo (evita que un Admin se quede afuera de su propio negocio).
    /// </summary>
    [HttpPatch("{id:int}/desactivar")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        var desactivado = await _usuarioAppService.DesactivarAsync(id, cancellationToken);
        return Ok(desactivado);
    }
}
