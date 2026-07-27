namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record ClienteAuthResponseDto(string Token, int TenantId, string Email);
