using TurnosApp.Core.Application.DTOs.Caja;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ICajaAppService
{
    Task<SesionCajaDto?> GetSesionAbiertaAsync(CancellationToken cancellationToken = default);

    Task<SesionCajaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<SesionCajaDto> AbrirSesionAsync(AbrirSesionCajaDto dto, CancellationToken cancellationToken = default);

    Task<MovimientoCajaDto> RegistrarMovimientoAsync(RegistrarMovimientoCajaDto dto, CancellationToken cancellationToken = default);

    Task<SesionCajaDto> CerrarSesionAsync(int id, CerrarSesionCajaDto dto, CancellationToken cancellationToken = default);

    Task<HistorialSesionesCajaDto> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Llamado por CobroAppService tras crear/editar un Cobro (mismo patrón que
    /// INotificacionAppService llamado desde TurnoAppService). No-op si no hay sesión
    /// abierta. Si el cobro ya tenía un movimiento automático en la sesión abierta,
    /// lo reversa y crea uno nuevo — MovimientoCaja es inmutable.
    /// </summary>
    Task SincronizarMovimientoDeCobroAsync(Cobro cobro, CancellationToken cancellationToken = default);
}
