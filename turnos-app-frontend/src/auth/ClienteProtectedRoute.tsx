// src/auth/ClienteProtectedRoute.tsx
import { Navigate, useLocation, useParams } from 'react-router-dom';
import type { ReactNode } from 'react';
import { Center, Loader } from '@mantine/core';
import { useClienteAuth } from '../context/useClienteAuth';

function isAuthenticated(): boolean {
  const token = localStorage.getItem('turnify_cliente_token');
  const tenantId = localStorage.getItem('turnify_cliente_tenant_id');
  return Boolean(token) && Boolean(tenantId);
}

export function ClienteProtectedRoute({ children }: { children: ReactNode }) {
  const { slug } = useParams<{ slug: string }>();
  const location = useLocation();
  const { loading } = useClienteAuth();

  if (!isAuthenticated()) {
    const returnTo = encodeURIComponent(`${location.pathname}${location.search}`);
    return <Navigate to={`/reservas/${slug}/login?returnTo=${returnTo}`} replace />;
  }

  if (loading) {
    return (
      <Center h="100vh">
        <Loader type="dots" />
      </Center>
    );
  }

  return <>{children}</>;
}
