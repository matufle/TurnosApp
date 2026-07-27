namespace TurnosApp.Core.Application.DTOs.Turnos;

// Sin ClienteId/ClienteNuevo: el cliente sale del JWT (ICurrentClienteService), nunca del
// body — evita que alguien reserve a nombre de otro cliente.
public record CrearTurnoPublicoDto(
    int RecursoId,
    IReadOnlyList<int> ServicioIds,
    DateTime FechaHoraInicio);
