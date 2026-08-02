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

    // Nullable: sólo los Cliente que se auto-registraron (self-service) tienen esto seteado.
    // Un Cliente cargado a mano por el staff (walk-in) puede seguir sin cuenta indefinidamente.
    public string? PasswordHash { get; set; }

    // Solo relevante cuando PasswordHash != null (ver LoginAsync: un walk-in sin cuenta
    // ya corta antes por PasswordHash is null).
    public bool EmailConfirmado { get; set; } = false;
    public string? TokenConfirmacionEmail { get; set; }
    public DateTime? TokenConfirmacionExpira { get; set; }
    public string? TokenResetPassword { get; set; }
    public DateTime? TokenResetPasswordExpira { get; set; }

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