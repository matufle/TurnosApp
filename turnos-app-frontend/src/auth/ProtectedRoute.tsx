import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';

function isAuthenticated(): boolean {
  // Placeholder — reemplazar cuando tengas el login real contra el backend
  return Boolean(localStorage.getItem('turnify_token'));
}

export function ProtectedRoute({ children }: { children: ReactNode }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}