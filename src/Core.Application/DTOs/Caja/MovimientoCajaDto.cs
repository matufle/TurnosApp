namespace TurnosApp.Core.Application.DTOs.Caja;

public record MovimientoCajaDto(
    int Id,
    string Tipo,
    decimal Monto,
    int? MetodoPagoId,
    string NombreMetodoPagoSnapshot,
    bool EsEfectivoSnapshot,
    string Concepto,
    DateTime FechaHora,
    int UsuarioId,
    string UsuarioNombre,
    int? CobroId,
    int? MovimientoOrigenId
);
