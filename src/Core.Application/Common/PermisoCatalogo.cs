using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Common;

/// <summary>
/// Catálogo de los permisos "atómicos" (un solo bit) del enum Permiso, usado para
/// convertir entre el bitmask guardado en Rol y la lista de nombres que ve el frontend
/// (ej. la grilla de checkboxes al crear/editar un rol).
/// </summary>
public static class PermisoCatalogo
{
    public static readonly IReadOnlyList<Permiso> Todos = new[]
    {
        Permiso.VerAgendaCompleta,
        Permiso.GestionarTurnos,
        Permiso.GestionarClientes,
        Permiso.GestionarServicios,
        Permiso.GestionarRecursos,
        Permiso.GestionarMetodosPago,
        Permiso.CrearCobros,
        Permiso.VerGananciaNeta,
        Permiso.VerReportes,
        Permiso.GestionarUsuarios,
        Permiso.GestionarRoles,
        Permiso.GestionarConfiguracionNegocio,
        Permiso.GestionarListaEspera,
    };

    public static IReadOnlyList<string> ToNombres(Permiso permisos) =>
        Todos.Where(p => permisos.HasFlag(p)).Select(p => p.ToString()).ToList();

    public static Permiso Parse(IEnumerable<string> nombres)
    {
        var resultado = Permiso.Ninguno;

        foreach (var nombre in nombres)
        {
            if (!Enum.TryParse<Permiso>(nombre, out var permiso) || !Todos.Contains(permiso))
                throw new BadRequestException($"Permiso desconocido: {nombre}");

            resultado |= permiso;
        }

        return resultado;
    }
}
