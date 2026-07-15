using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs.Tenant;
using TurnosApp.Core.Application.DTOs.Tenants;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

/// <summary>
/// Gestión global de Tenants (empresas/locales).
/// No requiere X-Tenant-Id — este controller opera a nivel global.
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

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantAppService.GetAllAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantAppService.GetByIdAsync(id, cancellationToken);
        return Ok(tenant);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _tenantAppService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // --- NUEVOS ENDPOINTS PARA EL ISSUE #16 (Configuración) ---

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