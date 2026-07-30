using TurnosApp.Core.Application.DTOs.Liquidaciones;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ILiquidacionAppService
{
    Task<IReadOnlyList<LiquidacionListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<LiquidacionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Self-service: liquidaciones del propio Recurso vinculado al usuario autenticado.</summary>
    Task<IReadOnlyList<LiquidacionListItemDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default);

    Task<LiquidacionDto> MarcarPagadaAsync(int id, MarcarPagadaLiquidacionDto dto, CancellationToken cancellationToken = default);

    Task<LiquidacionDto> AnularAsync(int id, AnularLiquidacionDto dto, CancellationToken cancellationToken = default);
}
