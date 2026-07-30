namespace TurnosApp.Core.Application.DTOs.Liquidaciones;

public record LiquidacionListItemDto(
    int Id,
    int RecursoId,
    string RecursoNombre,
    DateTime PeriodoDesde,
    DateTime PeriodoHasta,
    DateTime FechaGeneracion,
    string Estado,
    decimal MontoBrutoComision,
    decimal MontoAdelantos,
    decimal MontoNeto
);
