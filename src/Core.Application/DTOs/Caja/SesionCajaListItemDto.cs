namespace TurnosApp.Core.Application.DTOs.Caja;

// Versión liviana de SesionCajaDto para el historial paginado — sin el detalle de movimientos.
public record SesionCajaListItemDto(
    int Id,
    DateTime FechaApertura,
    DateTime? FechaCierre,
    decimal MontoInicial,
    decimal? MontoFinalDeclarado,
    decimal MontoEsperadoEfectivo,
    decimal? Diferencia,
    bool CierreForzado,
    int UsuarioAperturaId,
    string UsuarioAperturaNombre,
    int? UsuarioCierreId,
    string? UsuarioCierreNombre
);
