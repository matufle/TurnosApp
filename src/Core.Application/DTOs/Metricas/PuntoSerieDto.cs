namespace TurnosApp.Core.Application.DTOs.Metricas;

// Un punto de una serie temporal de un solo valor (línea/barra simple).
// Etiqueta ya viene formateada por el backend (fecha o nombre de bucket)
// para que el frontend no tenga que reconstruirla desde un string de fecha.
public record PuntoSerieDto(string Etiqueta, decimal Valor);
