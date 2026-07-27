// src/context/useClienteAuth.ts
import { useContext } from 'react';
import { ClienteAuthContext } from './ClienteAuthContext';

export function useClienteAuth() {
  const context = useContext(ClienteAuthContext);
  if (context === undefined) {
    throw new Error('useClienteAuth debe usarse dentro de un ClienteAuthProvider');
  }
  return context;
}
