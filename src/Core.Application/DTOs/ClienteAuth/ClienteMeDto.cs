namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record ClienteMeDto(int ClienteId, string Nombre, string Apellido, string Email, int TenantId);
