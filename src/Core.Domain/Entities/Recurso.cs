using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

public class Recurso : TenantEntity
{
    public string Nombre { get; set; } = string.Empty;        // ej: "Dr. García", "Sala 3"
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Turno> Turnos { get; set; } = [];
}