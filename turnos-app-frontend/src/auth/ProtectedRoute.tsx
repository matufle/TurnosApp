// src/auth/ProtectedRoute.tsx
import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';

function isAuthenticated(): boolean {
  const token = localStorage.getItem('turnify_token');
  const tenantId = localStorage.getItem('turnify_tenant_id');
  return Boolean(token) && Boolean(tenantId);
}

export function ProtectedRoute({ children }: { children: ReactNode }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}