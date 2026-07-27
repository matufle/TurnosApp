namespace TurnosApp.Core.Application.DTOs.ListaEspera;

public record CrearListaEsperaDto(
    int ClienteId,
    int RecursoId,
    int? ServicioId,
    DateTime FechaDesde,
    DateTime FechaHasta
);
