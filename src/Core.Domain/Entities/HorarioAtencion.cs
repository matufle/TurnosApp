namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

public class HorarioAtencion : TenantEntity
{
    public int RecursoId { get; set; }
    public Recurso Recurso { get; set; } = null!;

    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
}
