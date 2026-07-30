// src/layout/navLinks.ts
import {
  IconCalendarEvent,
  IconUsers,
  IconUserCog,
  IconBriefcase,
  IconSettings,
  IconCreditCard,
  IconReceipt2,
  IconUsersGroup,
  IconShieldLock,
  IconChartBar,
  IconHourglassHigh,
  IconClock,
  IconCashRegister,
  IconWallet,
} from '@tabler/icons-react';

// Compartido entre DashboardLayout (sidebar) y OnboardingTour (arma el tour en el mismo
// orden/permiso que el sidebar). En su propio módulo para no romper Fast Refresh en
// DashboardLayout.tsx (que debe exportar solo el componente).
export const navLinks = [
  { icon: IconCalendarEvent, label: 'Turnos', path: '/app/turnos' },
  { icon: IconUsers, label: 'Clientes', path: '/app/clientes' },
  { icon: IconHourglassHigh, label: 'Lista de Espera', path: '/app/lista-espera' },
  { icon: IconUserCog, label: 'Recursos', path: '/app/recursos' },
  { icon: IconClock, label: 'Horarios de Atención', path: '/app/recursos/horarios', permiso: 'GestionarRecursos' },
  { icon: IconBriefcase, label: 'Servicios', path: '/app/servicios' },
  { icon: IconCreditCard, label: 'Métodos de Pago', path: '/app/metodos-pago', permiso: 'GestionarMetodosPago' },
  { icon: IconReceipt2, label: 'Historial de Cobros', path: '/app/cobros' },
  // Array: alcanza con cualquiera de los dos — VerCaja (solo lectura) o GestionarCaja (opera
  // y por lo tanto también necesita verla). Evita el caso de un admin que tilda "Gestionar
  // Caja" para un cajero y se olvida de tildar "Ver Caja" aparte, dejando la página inaccesible.
  { icon: IconCashRegister, label: 'Caja', path: '/app/caja', permiso: ['VerCaja', 'GestionarCaja'] },
  { icon: IconWallet, label: 'Liquidaciones', path: '/app/liquidaciones', permiso: ['VerLiquidaciones', 'GestionarLiquidaciones'] },
  { icon: IconChartBar, label: 'Métricas', path: '/app/metricas', permiso: 'VerReportes' },
  { icon: IconUsersGroup, label: 'Usuarios', path: '/app/usuarios', permiso: 'GestionarUsuarios' },
  { icon: IconShieldLock, label: 'Roles y Permisos', path: '/app/roles', permiso: 'GestionarRoles' },
  { icon: IconSettings, label: 'Configuración', path: '/app/configuracion', permiso: 'GestionarConfiguracionNegocio' },
];

// Un link es visible si no exige permiso, o si el usuario tiene AL MENOS UNO de los
// indicados (soporta tanto un string suelto como un array, ver la entrada de Caja arriba).
export function esLinkVisible(permiso: string | string[] | undefined, hasPermission: (p: string) => boolean): boolean {
  if (!permiso) return true;
  return Array.isArray(permiso) ? permiso.some(hasPermission) : hasPermission(permiso);
}
