using TurnosApp.Core.Application.DTOs.ClienteAuth;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IClienteAuthAppService
{
    Task<ClienteAuthResponseDto> RegistrarAsync(ClienteRegistroDto dto, CancellationToken cancellationToken = default);
    Task<ClienteAuthResponseDto> LoginAsync(ClienteLoginDto dto, CancellationToken cancellationToken = default);
    Task<ClienteMeDto> GetMeAsync(CancellationToken cancellationToken = default);
}
