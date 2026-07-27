namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

public class ListaEspera : TenantEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int RecursoId { get; set; }
    public Recurso Recurso { get; set; } = null!;

    // Opcional: si no se especifica, cualquier turno liberado en el Recurso cuenta como coincidencia.
    public int? ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    // Ventana de fechas en la que el cliente acepta cualquier horario que se libere.
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }

    public EstadoListaEspera Estado { get; set; } = EstadoListaEspera.Activa;
    public DateTime? NotificadoEn { get; set; }
}
