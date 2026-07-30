using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs.ClienteAuth;

public record ConfirmarEmailClienteDto(
    [Required] string TenantSlug,
    [Required] string Token
);
