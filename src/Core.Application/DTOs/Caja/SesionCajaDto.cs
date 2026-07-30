namespace TurnosApp.Core.Application.DTOs.Caja;

public record SesionCajaDto(
    int Id,
    DateTime FechaApertura,
    DateTime? FechaCierre,
    decimal MontoInicial,
    decimal? MontoFinalDeclarado,
    decimal MontoEsperadoEfectivo,
    decimal? Diferencia,
    string Estado,
    bool CierreForzado,
    string? Observaciones,
    int UsuarioAperturaId,
    string UsuarioAperturaNombre,
    int? UsuarioCierreId,
    string? UsuarioCierreNombre,
    IReadOnlyList<MovimientoCajaDto> Movimientos,
    IReadOnlyList<DesgloseMedioPagoDto> DesglosePorMedioPago
);
