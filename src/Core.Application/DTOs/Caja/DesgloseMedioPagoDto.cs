namespace TurnosApp.Core.Application.DTOs.Caja;

// Efectivo es lo único con sentido de "declarado/esperado/diferencia" (SesionCajaDto):
// el resto de los medios de pago solo se muestra como total facturado, para conciliar
// después contra el resumen del banco/procesador — no se cuenta físicamente.
public record DesgloseMedioPagoDto(
    int? MetodoPagoId,
    string Nombre,
    bool EsEfectivo,
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal Total
);
