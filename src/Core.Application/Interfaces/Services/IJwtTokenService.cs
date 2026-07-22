using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(Usuario usuario, bool recordarme = false);
}
