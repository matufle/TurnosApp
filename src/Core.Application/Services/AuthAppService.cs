using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Core.Application.Services;

public interface IAuthAppService
{
    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken);
}

public class AuthAppService : IAuthAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthAppService(
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken)
    {
        // Gracias al query filter global, esto ya busca SOLO dentro del tenant
        // que llegó en el header X-Tenant-Id
        var usuario = await _unitOfWork.Usuarios.GetByEmailGlobalAsync(dto.Email, cancellationToken);

        if (usuario is null || !_passwordHasher.VerifyPassword(usuario.PasswordHash, dto.Password))
            throw new BusinessException(code: "CREDENCIALES_INVALIDAS", message: "Email o contraseña incorrectos.");

        var token = _jwtTokenService.GenerateToken(usuario);

        return new LoginResponseDTO(token, usuario.TenantId, usuario.Email);
    }
}
