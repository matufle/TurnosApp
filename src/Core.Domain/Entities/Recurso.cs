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

    public string ColorHex { get; set; } = "#0EA5E9";

    // Vínculo opcional: un Recurso puede no tener usuario logueable (ej. "Sala 3"),
    // y un Usuario (Admin/Recepcionista) puede no estar atado a ningún Recurso.
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // Navegación
    public ICollection<Turno> Turnos { get; set; } = [];
    public ICollection<ReglaComision> ReglasComision { get; set; } = [];
    public ICollection<Liquidacion> Liquidaciones { get; set; } = [];
    public ICollection<AdelantoProfesional> AdelantosProfesional { get; set; } = [];
}