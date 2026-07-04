using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs;

public record RegisterRequestDTO(string NombreNegocio, string Email, string Password);