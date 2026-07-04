using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs;

namespace TurnosApp.Core.Application.Interfaces.Services
{
    public interface IAuthAppService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken);
        Task<LoginResponseDTO> RegisterAsync(RegisterRequestDTO dto, CancellationToken cancellationToken);
    }
}
