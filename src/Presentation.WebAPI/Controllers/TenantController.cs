using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Tenant;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

/// <summary>
/// Configuración del tenant autenticado. Todas las operaciones son self-service:
/// el TenantId sale del JWT (ICurrentUserService), nunca de un parámetro de ruta,
/// para que un tenant no pueda leer ni modificar la configuración de otro.
/// No requiere X-Tenant-Id.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantAppService _tenantAppService;

    public TenantsController(ITenantAppService tenantAppService)
    {
        _tenantAppService = tenantAppService;
    }

    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Asegurate de tener TenantConfigDto creado
    public async Task<IActionResult> GetConfig(CancellationToken cancellationToken)
    {
        // Acá delegamos la responsabilidad al servicio de saber QUÉ tenant traer.
        var config = await _tenantAppService.GetConfigAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPut("config")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateConfig(
        [FromBody] UpdateTenantConfigDto dto,
        CancellationToken cancellationToken)
    {
        await _tenantAppService.UpdateConfigAsync(dto, cancellationToken);
        return NoContent();
    }
}