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
    GestionarListaEspera = 1L << 12,
    GestionarCaja = 1L << 13,
    ForzarCierreCaja = 1L << 14,
    VerCaja = 1L << 15,

    // Sin este permiso (ni GestionarLiquidaciones), un Recurso vinculado a Usuario igual
    // puede ver sus propias liquidaciones — es el mismo criterio de "mis turnos" self-service.
    VerLiquidaciones = 1L << 16,
    GestionarLiquidaciones = 1L << 17,

    // Crear/gestionar la suscripción del tenant (iniciar/cancelar el preapproval de Mercado Pago).
    GestionarSuscripcion = 1L << 18,

    Todos = VerAgendaCompleta | GestionarTurnos | GestionarClientes | GestionarServicios
        | GestionarRecursos | GestionarMetodosPago | CrearCobros | VerGananciaNeta
        | VerReportes | GestionarUsuarios | GestionarRoles | GestionarConfiguracionNegocio
        | GestionarListaEspera | GestionarCaja | ForzarCierreCaja | VerCaja
        | VerLiquidaciones | GestionarLiquidaciones | GestionarSuscripcion
}
