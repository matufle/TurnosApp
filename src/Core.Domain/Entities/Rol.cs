using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

public class Rol : TenantEntity
{
    public string Nombre { get; set; } = string.Empty;

    // Bitmask de Permiso. Se guarda como long porque los permisos son un catálogo fijo del código,
    // no una tabla — evita una tabla de join RolPermiso para un set de checkboxes.
    public long PermisosMask { get; set; }

    // El rol seedeado al crear el tenant (siempre con todos los permisos) no se puede editar ni
    // borrar, para que el tenant nunca se quede sin nadie con GestionarUsuarios/GestionarRoles.
    public bool EsSistema { get; set; }

    public Permiso Permisos
    {
        get => (Permiso)PermisosMask;
        set => PermisosMask = (long)value;
    }

    // Navegación
    public ICollection<Usuario> Usuarios { get; set; } = [];

    public bool Tiene(Permiso permiso) => (Permisos & permiso) == permiso;
}
