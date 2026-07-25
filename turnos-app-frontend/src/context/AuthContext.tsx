// src/context/AuthContext.tsx
import { createContext, useCallback, useEffect, useState, type ReactNode } from 'react';
import { authService, type MeResponse } from '../api/authService';

export interface AuthUser {
  usuarioId: number;
  nombre: string;
  email: string;
  tenantId: number;
  rolId: number;
  rolNombre: string;
  permisos: string[];
  recursoId: number | null;
}

export interface AuthContextValue {
  user: AuthUser | null;
  loading: boolean;
  hasPermission: (permiso: string) => boolean;
  login: (token: string, tenantId: number) => Promise<void>;
  logout: () => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function mapMeToUser(me: MeResponse): AuthUser {
  return {
    usuarioId: me.usuarioId,
    nombre: me.nombre,
    email: me.email,
    tenantId: me.tenantId,
    rolId: me.rolId,
    rolNombre: me.rolNombre,
    permisos: me.permisos,
    recursoId: me.recursoId,
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  // Resuelto contra /auth/me (no decodificado del JWT): un cambio de rol o una
  // desactivación aplican de inmediato la próxima vez que se hidrata el contexto.
  const cargarPerfil = useCallback(async () => {
    const token = localStorage.getItem('turnify_token');

    if (!token) {
      setUser(null);
      setLoading(false);
      return;
    }

    try {
      const me = await authService.me();
      setUser(mapMeToUser(me));
    } catch {
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarPerfil();
  }, [cargarPerfil]);

  const login = useCallback(
    async (token: string, tenantId: number) => {
      localStorage.setItem('turnify_token', token);
      localStorage.setItem('turnify_tenant_id', tenantId.toString());
      setLoading(true);
      await cargarPerfil();
    },
    [cargarPerfil]
  );

  const logout = useCallback(() => {
    localStorage.removeItem('turnify_token');
    localStorage.removeItem('turnify_tenant_id');
    setUser(null);
  }, []);

  const hasPermission = useCallback((permiso: string) => user?.permisos.includes(permiso) ?? false, [user]);

  return (
    <AuthContext.Provider value={{ user, loading, hasPermission, login, logout }}>{children}</AuthContext.Provider>
  );
}
