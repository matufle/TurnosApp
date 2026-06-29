using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

public class Tenant : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;        // ej: "clinica-del-sur"
    public bool PermitirSolapamiento { get; set; } = false;
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<Recurso> Recursos { get; set; } = [];
    public ICollection<Cliente> Clientes { get; set; } = [];
    public ICollection<Servicio> Servicios { get; set; } = [];
    public ICollection<Turno> Turnos { get; set; } = [];
}