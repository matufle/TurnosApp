using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record ReenviarConfirmacionClienteDto(
    [Required] string TenantSlug,
    [Required, EmailAddress, StringLength(200)] string Email
);
