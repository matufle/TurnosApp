using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs;

namespace TurnosApp.Core.Application.Interfaces.Services
{
    public interface IAuthAppService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken);
        Task<RegistroPendienteDto> RegisterAsync(RegisterRequestDTO dto, CancellationToken cancellationToken);
        Task ConfirmarEmailAsync(string token, CancellationToken cancellationToken);
        Task ReenviarConfirmacionAsync(string email, CancellationToken cancellationToken);
    }
}
