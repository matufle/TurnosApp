using TurnosApp.Core.Application.DTOs.AdelantosProfesional;

namespace TurnosApp.Core.Application.DTOs.Liquidaciones;

public record LiquidacionDto(
    int Id,
    int RecursoId,
    string RecursoNombre,
    DateTime PeriodoDesde,
    DateTime PeriodoHasta,
    DateTime FechaGeneracion,
    string Estado,
    DateTime? FechaPago,
    int? UsuarioPagoId,
    string? UsuarioPagoNombre,
    string? Observaciones,
    decimal MontoBrutoComision,
    decimal MontoAdelantos,
    decimal MontoNeto,
    IReadOnlyList<LiquidacionDetalleDto> Detalles,
    IReadOnlyList<AdelantoProfesionalDto> Adelantos
);
