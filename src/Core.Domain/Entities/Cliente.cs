using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

public class Cliente : TenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Campo flexible para metadatos específicos del rubro.
    /// Ejemplo radiología: { "obraSocial": "PAMI", "nroAfiliado": "123" }
    /// Ejemplo peluquería: { "colorCabello": "castaño", "alergias": "ninguna" }
    /// Mapeado como columna JSON en SQL Server vía EF Core.
    /// </summary>
    public string? DatosEspecificosJson { get; set; }

    // Navegación
    public ICollection<Turno> Turnos { get; set; } = [];
}