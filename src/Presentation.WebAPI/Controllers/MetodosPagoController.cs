using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.MetodosPago;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class MetodosPagoController : ControllerBase
{
    private readonly IMetodoPagoService _metodoPagoService;

    public MetodosPagoController(IMetodoPagoService metodoPagoService)
    {
        _metodoPagoService = metodoPagoService;
    }

    /// <summary>Obtiene todos los métodos de pago del tenant actual (incluye inactivos).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MetodoPagoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var metodosPago = await _metodoPagoService.GetAllAsync(cancellationToken);
        return Ok(metodosPago);
    }

    /// <summary>Obtiene un método de pago por su ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MetodoPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var metodoPago = await _metodoPagoService.GetByIdAsync(id, cancellationToken);
        return Ok(metodoPago);
    }

    /// <summary>Crea un nuevo método de pago.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MetodoPagoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMetodoPagoDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _metodoPagoService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    /// <summary>Actualiza un método de pago existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MetodoPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMetodoPagoDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _metodoPagoService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Baja lógica (soft delete). Nunca se expone un DELETE físico:
    /// romper la FK histórica de Cobro.MetodoPagoId destruiría el snapshot de cobros pasados.
    /// </summary>
    [HttpPatch("{id:int}/desactivar")]
    [ProducesResponseType(typeof(MetodoPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        var desactivado = await _metodoPagoService.DesactivarAsync(id, cancellationToken);
        return Ok(desactivado);
    }
}
