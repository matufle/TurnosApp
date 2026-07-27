namespace TurnosApp.Core.Application.DTOs.ListaEspera;

public record ListaEsperaDto(
    int Id,
    int ClienteId,
    string ClienteNombreCompleto,
    int RecursoId,
    string RecursoNombre,
    int? ServicioId,
    string? ServicioNombre,
    DateTime FechaDesde,
    DateTime FechaHasta,
    string Estado,
    DateTime? NotificadoEn
);
