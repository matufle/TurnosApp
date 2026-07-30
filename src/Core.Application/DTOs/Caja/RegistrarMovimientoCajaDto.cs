namespace TurnosApp.Core.Application.DTOs.Caja;

public record RegistrarMovimientoCajaDto(
    string Tipo,             // "Ingreso" | "Egreso"
    decimal Monto,
    int MetodoPagoId,
    string Concepto
);
