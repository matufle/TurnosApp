// src/context/TenantThemeContext.tsx
import { createContext, useState, useCallback, type ReactNode } from 'react';

export interface TenantThemeContextValue {
  colorHex: string | null;
  setColorHex: (hex: string) => void;
}
// eslint-disable-next-line react-refresh/only-export-components
export const TenantThemeContext = createContext<TenantThemeContextValue | undefined>(undefined);

export function TenantThemeProvider({
  children,
  initialColorHex,
}: {
  children: ReactNode;
  initialColorHex: string | null;
}) {
  const [colorHex, setColorHexState] = useState<string | null>(initialColorHex);

  const setColorHex = useCallback((hex: string) => {
    setColorHexState(hex);
  }, []);

  return (
    <TenantThemeContext.Provider value={{ colorHex, setColorHex }}>
      {children}
    </TenantThemeContext.Provider>
  );
}