using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record ClienteLoginDto(
    [Required] string TenantSlug,
    [Required, EmailAddress] string Email,
    [Required] string Password,
    bool RecordarMe = false
);
