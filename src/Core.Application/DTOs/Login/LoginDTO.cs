using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TurnosApp.Core.Application.DTOs;

public record LoginRequestDTO(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(200, MinimumLength = 1)] string Password,
    bool RecordarMe = false
);

public record LoginResponseDTO(string Token, int TenantId, string Email);
