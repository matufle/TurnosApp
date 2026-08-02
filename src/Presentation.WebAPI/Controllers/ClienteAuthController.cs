using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TurnosApp.Core.Application.DTOs.ClienteAuth;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/cliente-auth")]
[Authorize]
public class ClienteAuthController : ControllerBase
{
    private readonly IClienteAuthAppService _clienteAuthAppService;

    public ClienteAuthController(IClienteAuthAppService clienteAuthAppService)
    {
        _clienteAuthAppService = clienteAuthAppService;
    }

    /// <summary>Perfil del cliente autenticado, para hidratar el ClienteAuthContext del frontend.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ClienteMeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteMeDto>> Me(CancellationToken cancellationToken)
    {
        var me = await _clienteAuthAppService.GetMeAsync(cancellationToken);
        return Ok(me);
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthRegister")]
    public async Task<ActionResult<ClienteRegistroPendienteDto>> Registro(
        [FromBody] ClienteRegistroDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _clienteAuthAppService.RegistrarAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    public async Task<ActionResult<ClienteAuthResponseDto>> Login(
        [FromBody] ClienteLoginDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _clienteAuthAppService.LoginAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("confirmar-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarEmail(
        [FromBody] ConfirmarEmailClienteDto dto,
        CancellationToken cancellationToken)
    {
        await _clienteAuthAppService.ConfirmarEmailAsync(dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("reenviar-confirmacion")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthRegister")]
    public async Task<IActionResult> ReenviarConfirmacion(
        [FromBody] ReenviarConfirmacionClienteDto dto,
        CancellationToken cancellationToken)
    {
        await _clienteAuthAppService.ReenviarConfirmacionAsync(dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("olvide-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthRegister")]
    public async Task<IActionResult> OlvidePassword(
        [FromBody] OlvidePasswordClienteDto dto,
        CancellationToken cancellationToken)
    {
        await _clienteAuthAppService.OlvidePasswordAsync(dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthRegister")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordClienteDto dto,
        CancellationToken cancellationToken)
    {
        await _clienteAuthAppService.ResetPasswordAsync(dto, cancellationToken);
        return NoContent();
    }
}
