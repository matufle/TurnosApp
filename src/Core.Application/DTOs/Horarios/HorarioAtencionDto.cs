namespace TurnosApp.Core.Application.DTOs.Horarios;

public record HorarioAtencionDto(int Id, DayOfWeek DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin);

public record HorarioAtencionItemDto(DayOfWeek DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin);

public record ReemplazarHorariosDto(IReadOnlyList<HorarioAtencionItemDto> Horarios);
