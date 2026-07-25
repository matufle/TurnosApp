using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Application.Interfaces.Services;

namespace TurnosApp.Core.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Usuario usuario, bool recordarme = false)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new("tenantId", usuario.TenantId.ToString()), // claim custom, clave para el multi-tenant
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("TenantId", usuario.TenantId.ToString()),
            // Claim custom (no "sub"): el JwtBearerHandler remapea "sub" a ClaimTypes.NameIdentifier
            // por el inbound claim mapping default — leerlo de vuelta como "sub" sería ambiguo.
            new("UsuarioId", usuario.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var vigencia = recordarme ? TimeSpan.FromDays(30) : TimeSpan.FromHours(8);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(vigencia),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
