using TurnosApp.Core.Application.DTOs.ClienteAuth;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IClienteAuthAppService
{
    Task<ClienteRegistroPendienteDto> RegistrarAsync(ClienteRegistroDto dto, CancellationToken cancellationToken = default);
    Task<ClienteAuthResponseDto> LoginAsync(ClienteLoginDto dto, CancellationToken cancellationToken = default);
    Task<ClienteMeDto> GetMeAsync(CancellationToken cancellationToken = default);
    Task ConfirmarEmailAsync(ConfirmarEmailClienteDto dto, CancellationToken cancellationToken = default);
    Task ReenviarConfirmacionAsync(ReenviarConfirmacionClienteDto dto, CancellationToken cancellationToken = default);
    Task OlvidePasswordAsync(OlvidePasswordClienteDto dto, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordClienteDto dto, CancellationToken cancellationToken = default);
}
