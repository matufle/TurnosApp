using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record OlvidePasswordClienteDto(
    [Required] string TenantSlug,
    [Required, EmailAddress, StringLength(200)] string Email
);
