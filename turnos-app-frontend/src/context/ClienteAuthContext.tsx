// src/context/ClienteAuthContext.tsx
import { createContext, useCallback, useEffect, useState, type ReactNode } from 'react';
import { clienteAuthService } from '../api/clienteAuthService';
import type { ClienteMeResponse } from '../types/ClienteAuth';

export interface ClienteAuthUser {
  clienteId: number;
  nombre: string;
  apellido: string;
  email: string;
  tenantId: number;
}

export interface ClienteAuthContextValue {
  cliente: ClienteAuthUser | null;
  loading: boolean;
  login: (token: string, tenantId: number, slug: string) => Promise<void>;
  logout: () => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const ClienteAuthContext = createContext<ClienteAuthContextValue | undefined>(undefined);

function mapMeToCliente(me: ClienteMeResponse): ClienteAuthUser {
  return {
    clienteId: me.clienteId,
    nombre: me.nombre,
    apellido: me.apellido,
    email: me.email,
    tenantId: me.tenantId,
  };
}

export function ClienteAuthProvider({ children }: { children: ReactNode }) {
  const [cliente, setCliente] = useState<ClienteAuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  const cargarPerfil = useCallback(async () => {
    const token = localStorage.getItem('turnify_cliente_token');

    if (!token) {
      setCliente(null);
      setLoading(false);
      return;
    }

    try {
      const me = await clienteAuthService.me();
      setCliente(mapMeToCliente(me));
    } catch {
      setCliente(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarPerfil();
  }, [cargarPerfil]);

  const login = useCallback(
    async (token: string, tenantId: number, slug: string) => {
      localStorage.setItem('turnify_cliente_token', token);
      localStorage.setItem('turnify_cliente_tenant_id', tenantId.toString());
      localStorage.setItem('turnify_cliente_slug', slug);
      setLoading(true);
      await cargarPerfil();
    },
    [cargarPerfil]
  );

  const logout = useCallback(() => {
    localStorage.removeItem('turnify_cliente_token');
    localStorage.removeItem('turnify_cliente_tenant_id');
    localStorage.removeItem('turnify_cliente_slug');
    setCliente(null);
  }, []);

  return (
    <ClienteAuthContext.Provider value={{ cliente, loading, login, logout }}>
      {children}
    </ClienteAuthContext.Provider>
  );
}
