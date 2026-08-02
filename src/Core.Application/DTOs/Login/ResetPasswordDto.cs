using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs;

public record ResetPasswordDto(
    [Required] string Token,
    [Required, StringLength(100, MinimumLength = 8)] string NuevaPassword
);
