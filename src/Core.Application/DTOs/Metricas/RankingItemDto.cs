namespace TurnosApp.Core.Application.DTOs.Metricas;

// Un ítem de un ranking "top N" (servicios, recursos, clientes).
// Valor es el monto (ingreso/facturación); Cantidad es el conteo (reservas/turnos).
public record RankingItemDto(int Id, string Nombre, decimal Valor, int Cantidad);
