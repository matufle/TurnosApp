using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

public class Servicio : TenantEntity
{
    public string Nombre { get; set; } = string.Empty;          // ej: "Consulta", "Radiografía"
    public string? Descripcion { get; set; }
    public int DuracionMinutos { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;

    // Navegación (M:N hacia Turno vía tabla de unión explícita)
    public ICollection<TurnoServicio> TurnoServicios { get; set; } = [];
}