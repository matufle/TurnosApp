using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TurnosApp.Core.Application.DTOs;

public record RegisterRequestDTO(
    [property: Required, StringLength(200, MinimumLength = 1)] string NombreNegocio,
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(100, MinimumLength = 8)] string Password
);