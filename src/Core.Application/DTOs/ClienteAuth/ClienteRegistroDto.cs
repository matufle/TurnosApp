using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record ClienteRegistroDto(
    [Required] string TenantSlug,
    [Required, StringLength(150, MinimumLength = 1)] string Nombre,
    [Required, StringLength(150, MinimumLength = 1)] string Apellido,
    [Required, EmailAddress, StringLength(200)] string Email,
    string? Telefono,
    [Required, StringLength(100, MinimumLength = 8)] string Password
);
