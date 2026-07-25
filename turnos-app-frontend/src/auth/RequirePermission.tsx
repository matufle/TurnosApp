// src/auth/RequirePermission.tsx
import type { ReactNode } from 'react';
import { usePermission } from '../hooks/usePermission';

/**
 * Oculta sus children si el usuario actual no tiene el permiso indicado.
 * Uso: <RequirePermission permiso="GestionarUsuarios"><Boton /></RequirePermission>
 */
export function RequirePermission({ permiso, children }: { permiso: string; children: ReactNode }) {
  const tienePermiso = usePermission(permiso);
  if (!tienePermiso) return null;
  return <>{children}</>;
}
