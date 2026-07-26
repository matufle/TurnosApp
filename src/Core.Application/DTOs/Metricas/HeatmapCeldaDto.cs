namespace TurnosApp.Core.Application.DTOs.Metricas;

// DiaSemana: 0=Domingo..6=Sábado (System.DayOfWeek). Hora: 0-23.
public record HeatmapCeldaDto(int DiaSemana, int Hora, int Cantidad);
