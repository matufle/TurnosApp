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
} from '@tabler/icons-react';

// Compartido entre DashboardLayout (sidebar) y OnboardingTour (arma el tour en el mismo
// orden/permiso que el sidebar). En su propio módulo para no romper Fast Refresh en
// DashboardLayout.tsx (que debe exportar solo el componente).
export const navLinks = [
  { icon: IconCalendarEvent, label: 'Turnos', path: '/app/turnos' },
  { icon: IconUsers, label: 'Clientes', path: '/app/clientes' },
  { icon: IconHourglassHigh, label: 'Lista de Espera', path: '/app/lista-espera' },
  { icon: IconUserCog, label: 'Recursos', path: '/app/recursos' },
  { icon: IconBriefcase, label: 'Servicios', path: '/app/servicios' },
  { icon: IconCreditCard, label: 'Métodos de Pago', path: '/app/metodos-pago', permiso: 'GestionarMetodosPago' },
  { icon: IconReceipt2, label: 'Historial de Cobros', path: '/app/cobros' },
  { icon: IconChartBar, label: 'Métricas', path: '/app/metricas', permiso: 'VerReportes' },
  { icon: IconUsersGroup, label: 'Usuarios', path: '/app/usuarios', permiso: 'GestionarUsuarios' },
  { icon: IconShieldLock, label: 'Roles y Permisos', path: '/app/roles', permiso: 'GestionarRoles' },
  { icon: IconSettings, label: 'Configuración', path: '/app/configuracion', permiso: 'GestionarConfiguracionNegocio' },
];
