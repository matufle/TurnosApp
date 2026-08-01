using TurnosApp.Core.Application.DTOs.Suscripcion;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ISuscripcionAppService
{
    Task<SuscripcionDto> GetEstadoAsync(CancellationToken cancellationToken = default);

    /// <summary>Crea el preapproval en Mercado Pago y devuelve la URL de checkout hospedada a la que redirigir.</summary>
    Task<string> IniciarSuscripcionAsync(CancellationToken cancellationToken = default);

    /// <summary>Cancela el preapproval activo — MP no tiene un portal de autogestión, la cancelación se pide acá.</summary>
    Task CancelarSuscripcionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invocado desde el webhook de Mercado Pago (MP es el caller, no un usuario autenticado
    /// — de ahí que reciba el preapprovalId en vez de resolverlo por JWT).
    /// </summary>
    Task ActualizarDesdeNotificacionAsync(string preapprovalId, CancellationToken cancellationToken = default);
}
