using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record ResetPasswordClienteDto(
    [Required] string TenantSlug,
    [Required] string Token,
    [Required, StringLength(100, MinimumLength = 8)] string NuevaPassword
);
