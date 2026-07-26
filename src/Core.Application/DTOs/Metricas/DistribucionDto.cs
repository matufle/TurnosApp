namespace TurnosApp.Core.Application.DTOs.Metricas;

// Un segmento de un gráfico de dona (turnos por estado, estado de pago, clientes por tipo).
public record DistribucionDto(string Categoria, int Cantidad, decimal Porcentaje);
