using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TurnosApp.Core.Application.DTOs;

public record LoginRequestDTO(
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(200, MinimumLength = 1)] string Password
);

public record LoginResponseDTO(string Token, int TenantId, string Email);
