using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

public class Turno : TenantEntity
{
    public int RecursoId { get; set; }
    public int ClienteId { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    /// <summary>
    /// Calculada en dominio: FechaHoraInicio + suma de DuracionMinutos de todos los servicios.
    /// No persistida en base de datos.
    /// </summary>
    [NotMapped]
    public DateTime FechaHoraFin =>
        FechaHoraInicio.AddMinutes(
            TurnoServicios.Sum(ts => ts.Servicio?.DuracionMinutos ?? 0)
        );

    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;
    public string? Notas { get; set; }

    // Auditoría
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ModificadoEn { get; set; }
    public string? CreadoPor { get; set; }                     // UserId como string (JWT sub)

    // Navegación
    public Recurso Recurso { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public ICollection<TurnoServicio> TurnoServicios { get; set; } = [];
}