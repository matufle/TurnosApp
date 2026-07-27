using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs;
using TurnosApp.Core.Application.DTOs.Auth;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Application.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;
    private readonly IUsuarioAppService _usuarioAppService;

    public AuthController(IAuthAppService authAppService, IUsuarioAppService usuarioAppService)
    {
        _authAppService = authAppService;
        _usuarioAppService = usuarioAppService;
    }

    /// <summary>Perfil del usuario autenticado + sus permisos, para hidratar el AuthContext del frontend.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MeDto>> Me(CancellationToken cancellationToken)
    {
        var me = await _usuarioAppService.GetMeAsync(cancellationToken);
        return Ok(me);
    }

    /// <summary>Marca el onboarding del usuario autenticado como completado (fin o skip del tour).</summary>
    [HttpPatch("onboarding")]
    public async Task<IActionResult> CompletarOnboarding(CancellationToken cancellationToken)
    {
        await _usuarioAppService.CompletarOnboardingAsync(cancellationToken);
        return NoContent();
    }
    //Estos son los endpoints de autenticación, que permiten a los usuarios iniciar sesión y registrarse en la aplicación.
    [HttpPost("login")]
    [AllowAnonymous] // clave: este endpoint no requiere JWT (obviamente, todavía no lo tiene)
    public async Task<ActionResult<LoginResponseDTO>> Login(
        [FromBody] LoginRequestDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _authAppService.LoginAsync(dto, cancellationToken);
        return Ok(result);
    }
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Register(
    [FromBody] RegisterRequestDTO dto,
    CancellationToken cancellationToken)
    {
        var result = await _authAppService.RegisterAsync(dto, cancellationToken);
        return Ok(result);
    }
}
