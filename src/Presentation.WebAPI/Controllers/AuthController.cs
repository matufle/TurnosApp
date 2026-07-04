using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.DTOs;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Application.Services;

namespace TurnosApp.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
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
