using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

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

    public async Task<LoginResponseDTO> RegisterAsync(RegisterRequestDTO dto, CancellationToken cancellationToken)
    {
        var emailExistente = await _unitOfWork.Usuarios.GetByEmailGlobalAsync(dto.Email, cancellationToken);

        if (emailExistente is not null)
            throw new ConflictException("Ya existe una cuenta registrada con ese email.");

        var tenant = new Tenant
        {
            Nombre = dto.NombreNegocio,
            Slug = GenerarSlug(dto.NombreNegocio),
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // necesario: EF recién asigna tenant.Id acá

        var passwordHash = _passwordHasher.HashPassword(dto.Password);
        var usuario = new Usuario(dto.Email, passwordHash, tenant.Id);

        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(usuario);

        return new LoginResponseDTO(token, usuario.TenantId, usuario.Email);
    }

    private static string GenerarSlug(string nombre)
    {
        return nombre
            .ToLowerInvariant()
            .Trim()
            .Replace(" ", "-")
            .Normalize(System.Text.NormalizationForm.FormD); // simplificado — ver nota abajo
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByEmailGlobalAsync(dto.Email, cancellationToken);

        if (usuario is null || !_passwordHasher.VerifyPassword(usuario.PasswordHash, dto.Password))
            throw new BusinessException(code: "CREDENCIALES_INVALIDAS", message: "Email o contraseña incorrectos.");

        var token = _jwtTokenService.GenerateToken(usuario, dto.RecordarMe);

        return new LoginResponseDTO(token, usuario.TenantId, usuario.Email);
    }
}
