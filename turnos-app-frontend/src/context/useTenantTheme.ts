// src/context/useTenantTheme.ts
import { useContext } from 'react';
import { TenantThemeContext } from './TenantThemeContext';

export function useTenantTheme() {
  const context = useContext(TenantThemeContext);
  if (context === undefined) {
    throw new Error('useTenantTheme debe usarse dentro de un TenantThemeProvider');
  }
  return context;
}