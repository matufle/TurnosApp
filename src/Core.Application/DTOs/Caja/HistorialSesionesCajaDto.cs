namespace TurnosApp.Core.Application.DTOs.Caja;

public record HistorialSesionesCajaDto(
    IReadOnlyList<SesionCajaListItemDto> Items,
    int TotalCount,
    int Pagina,
    int TamanoPagina
);
