using System;

namespace TurnosApp.Core.Domain.Enums;

[Flags]
public enum Permiso : long
{
    Ninguno = 0,

    // Sin este permiso, el usuario queda acotado a sus propios turnos (los de su Recurso vinculado).
    VerAgendaCompleta = 1L << 0,
    GestionarTurnos = 1L << 1,
    GestionarClientes = 1L << 2,
    GestionarServicios = 1L << 3,
    GestionarRecursos = 1L << 4,
    GestionarMetodosPago = 1L << 5,
    CrearCobros = 1L << 6,
    VerGananciaNeta = 1L << 7,
    VerReportes = 1L << 8,
    GestionarUsuarios = 1L << 9,
    GestionarRoles = 1L << 10,
    GestionarConfiguracionNegocio = 1L << 11,

    Todos = VerAgendaCompleta | GestionarTurnos | GestionarClientes | GestionarServicios
        | GestionarRecursos | GestionarMetodosPago | CrearCobros | VerGananciaNeta
        | VerReportes | GestionarUsuarios | GestionarRoles | GestionarConfiguracionNegocio
}
