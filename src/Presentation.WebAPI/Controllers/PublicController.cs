using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Public;
using TurnosApp.Core.Application.DTOs.Servicios;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

/// <summary>
/// Endpoints anónimos de cara al cliente final (self-service). Sin X-Tenant-Id
/// disponible todavía — exento en TenantMiddleware.
/// </summary>
[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly IPublicAppService _publicAppService;
    private readonly IPublicCatalogoAppService _publicCatalogoAppService;

    public PublicController(IPublicAppService publicAppService, IPublicCatalogoAppService publicCatalogoAppService)
    {
        _publicAppService = publicAppService;
        _publicCatalogoAppService = publicCatalogoAppService;
    }

    [HttpGet("tenants/{slug}")]
    [ProducesResponseType(typeof(TenantPublicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantPorSlug(string slug, CancellationToken cancellationToken)
    {
        var tenant = await _publicAppService.ResolverTenantPorSlugAsync(slug, cancellationToken);
        return Ok(tenant);
    }

    [HttpGet("tenants/{slug}/servicios")]
    [ProducesResponseType(typeof(IReadOnlyList<ServicioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServicios(string slug, CancellationToken cancellationToken)
    {
        var servicios = await _publicCatalogoAppService.GetServiciosAsync(slug, cancellationToken);
        return Ok(servicios);
    }

    [HttpGet("tenants/{slug}/recursos")]
    [ProducesResponseType(typeof(IReadOnlyList<RecursoPublicoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecursos(string slug, CancellationToken cancellationToken)
    {
        var recursos = await _publicCatalogoAppService.GetRecursosAsync(slug, cancellationToken);
        return Ok(recursos);
    }

    [HttpGet("tenants/{slug}/disponibilidad")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDisponibilidad(
        string slug,
        [FromQuery] int recursoId,
        [FromQuery] int servicioId,
        [FromQuery] DateOnly fecha,
        CancellationToken cancellationToken)
    {
        var slots = await _publicCatalogoAppService.GetDisponibilidadAsync(slug, recursoId, servicioId, fecha, cancellationToken);
        return Ok(slots);
    }
}
