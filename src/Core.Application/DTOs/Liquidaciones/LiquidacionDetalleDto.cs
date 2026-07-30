namespace TurnosApp.Core.Application.DTOs.Liquidaciones;

public record LiquidacionDetalleDto(
    int Id,
    int TurnoId,
    DateTime TurnoFecha,
    int ServicioId,
    string ServicioNombre,
    decimal PrecioBaseAplicado,
    string TipoComisionSnapshot,
    decimal ValorComisionSnapshot,
    decimal MontoComisionCalculado
);
