using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.ListaEspera;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ListaEsperaController : ControllerBase
{
    private readonly IListaEsperaAppService _listaEsperaAppService;

    public ListaEsperaController(IListaEsperaAppService listaEsperaAppService)
    {
        _listaEsperaAppService = listaEsperaAppService;
    }

    /// <summary>Obtiene todas las entradas de lista de espera del tenant actual.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ListaEsperaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var entradas = await _listaEsperaAppService.GetAllAsync(cancellationToken);
        return Ok(entradas);
    }

    /// <summary>Anota a un cliente en lista de espera para un Recurso (y opcionalmente Servicio) en un rango de fechas.</summary>
    [HttpPost]
    [RequierePermiso(Permiso.GestionarListaEspera)]
    [ProducesResponseType(typeof(ListaEsperaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] CrearListaEsperaDto dto, CancellationToken cancellationToken)
    {
        var creada = await _listaEsperaAppService.CrearAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, creada);
    }

    /// <summary>Cancela una entrada de lista de espera (no se borra físicamente).</summary>
    [HttpPatch("{id:int}/cancelar")]
    [RequierePermiso(Permiso.GestionarListaEspera)]
    [ProducesResponseType(typeof(ListaEsperaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
    {
        var cancelada = await _listaEsperaAppService.CancelarAsync(id, cancellationToken);
        return Ok(cancelada);
    }
}
