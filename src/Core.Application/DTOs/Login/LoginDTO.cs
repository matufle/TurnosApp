using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs;

public record LoginRequestDTO(string Email, string Password);

public record LoginResponseDTO(string Token, int TenantId, string Email);
