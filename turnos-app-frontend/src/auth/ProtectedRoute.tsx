// src/auth/ProtectedRoute.tsx
import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';
import { Center, Loader } from '@mantine/core';
import { useAuth } from '../context/useAuth';

function isAuthenticated(): boolean {
  const token = localStorage.getItem('turnify_token');
  const tenantId = localStorage.getItem('turnify_tenant_id');
  return Boolean(token) && Boolean(tenantId);
}

export function ProtectedRoute({ children, permiso }: { children: ReactNode; permiso?: string }) {
  const { loading, hasPermission } = useAuth();

  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  if (loading) {
    return (
      <Center h="100vh">
        <Loader type="dots" />
      </Center>
    );
  }

  if (permiso && !hasPermission(permiso)) {
    return <Navigate to="/app" replace />;
  }

  return <>{children}</>;
}
