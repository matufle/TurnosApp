using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Suscripcion;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Presentation.WebAPI.Authorization;

namespace TurnosApp.Presentation.WebAPI.Controllers;

/// <summary>
/// Suscripción del tenant autenticado (self-service, TenantId sale del JWT — mismo
/// criterio que TenantsController). El estado (GET) queda abierto a cualquier usuario
/// autenticado para que la UI pueda mostrar el aviso de "suscripción vencida" sin
/// necesitar GestionarSuscripcion; las acciones que tocan Mercado Pago sí lo requieren.
/// </summary>
[ApiController]
[Route("api/suscripciones")]
[Produces("application/json")]
[Authorize]
public class SuscripcionesController : ControllerBase
{
    private readonly ISuscripcionAppService _suscripcionAppService;

    public SuscripcionesController(ISuscripcionAppService suscripcionAppService)
    {
        _suscripcionAppService = suscripcionAppService;
    }

    [HttpGet("estado")]
    [ProducesResponseType(typeof(SuscripcionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SuscripcionDto>> GetEstado(CancellationToken cancellationToken)
    {
        var estado = await _suscripcionAppService.GetEstadoAsync(cancellationToken);
        return Ok(estado);
    }

    [HttpPost("iniciar")]
    [RequierePermiso(Permiso.GestionarSuscripcion)]
    [ProducesResponseType(typeof(IniciarSuscripcionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IniciarSuscripcionDto>> Iniciar(CancellationToken cancellationToken)
    {
        var url = await _suscripcionAppService.IniciarSuscripcionAsync(cancellationToken);
        return Ok(new IniciarSuscripcionDto(url));
    }

    [HttpPost("cancelar")]
    [RequierePermiso(Permiso.GestionarSuscripcion)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancelar(CancellationToken cancellationToken)
    {
        await _suscripcionAppService.CancelarSuscripcionAsync(cancellationToken);
        return NoContent();
    }
}
