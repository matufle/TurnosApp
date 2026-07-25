// Coincide con el enum Permiso del backend (src/Core.Domain/Enums/Permiso.cs).
// Agrupado por área para la grilla de checkboxes de RolesPage.
export type Permiso =
  | 'VerAgendaCompleta'
  | 'GestionarTurnos'
  | 'GestionarClientes'
  | 'GestionarServicios'
  | 'GestionarRecursos'
  | 'GestionarMetodosPago'
  | 'CrearCobros'
  | 'VerGananciaNeta'
  | 'VerReportes'
  | 'GestionarUsuarios'
  | 'GestionarRoles'
  | 'GestionarConfiguracionNegocio';

export const PERMISOS_POR_AREA: { area: string; permisos: { valor: Permiso; label: string }[] }[] = [
  {
    area: 'Turnos y agenda',
    permisos: [
      { valor: 'VerAgendaCompleta', label: 'Ver la agenda de todos los recursos (sin esto, solo ve la propia)' },
      { valor: 'GestionarTurnos', label: 'Crear, editar y cancelar turnos' },
    ],
  },
  {
    area: 'Clientes y catálogo',
    permisos: [
      { valor: 'GestionarClientes', label: 'Gestionar clientes' },
      { valor: 'GestionarServicios', label: 'Gestionar servicios' },
      { valor: 'GestionarRecursos', label: 'Gestionar recursos (profesionales, salas)' },
    ],
  },
  {
    area: 'Cobros y finanzas',
    permisos: [
      { valor: 'CrearCobros', label: 'Registrar y editar cobros' },
      { valor: 'GestionarMetodosPago', label: 'Gestionar métodos de pago' },
      { valor: 'VerGananciaNeta', label: 'Ver la ganancia neta del negocio' },
      { valor: 'VerReportes', label: 'Ver reportes y métricas' },
    ],
  },
  {
    area: 'Equipo y negocio',
    permisos: [
      { valor: 'GestionarUsuarios', label: 'Gestionar usuarios del equipo' },
      { valor: 'GestionarRoles', label: 'Gestionar roles y permisos' },
      { valor: 'GestionarConfiguracionNegocio', label: 'Gestionar la configuración del negocio' },
    ],
  },
];
