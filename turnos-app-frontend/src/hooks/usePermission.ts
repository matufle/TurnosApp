// src/hooks/usePermission.ts
import { useAuth } from '../context/useAuth';

export function usePermission(permiso: string): boolean {
  const { hasPermission } = useAuth();
  return hasPermission(permiso);
}
