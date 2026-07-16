// src/App.tsx
import { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { MantineProvider, createTheme, Loader, Center } from '@mantine/core';

import { theme as turnifyTheme } from './theme/turnifyTheme';
import { generateShades } from './theme/generateShades';
import { TenantThemeProvider } from './context/TenantThemeContext';
import { useTenantTheme } from './context/useTenantTheme';
import { tenantService } from './api/tenantService';

import { LandingPage } from './pages/Landing/LandindPage';
import { DashboardLayout } from './layout/DashboardLayout';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { LoginPage } from './pages/Login/LoginPage';
import { RegisterPage } from './pages/Register/RegisterPage';
import { RecursosPage } from './pages/Recursos/RecursosPage';
import { ServiciosPage } from './pages/Servicios/ServiciosPage';
import { TurnosPage } from './pages/Turnos/TurnosPage';
import { ClientesPage } from './pages/Clientes/ClientesPage';
import { ConfigurationPage } from './pages/Configuration/ConfigurationPage';

function ThemedApp() {
  const { colorHex } = useTenantTheme();

const finalTheme =
    colorHex && colorHex !== '#0EA5E9'
      ? createTheme({
          ...turnifyTheme,
          primaryColor: 'cyan', // <-- ESTA LÍNEA ES LA CLAVE
          colors: {
            ...turnifyTheme.colors,
            cyan: generateShades(colorHex),
          },
          autoContrast: true,
          luminanceThreshold: 0.45,
        })
      : createTheme({
          ...turnifyTheme,
          primaryColor: 'cyan', // Lo aseguramos también para el tema por defecto
        });

  return (
    <MantineProvider theme={finalTheme}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/registro" element={<RegisterPage />} />

          <Route
            path="/app"
            element={
              <ProtectedRoute>
                <DashboardLayout />
              </ProtectedRoute>
            }
          >
            <Route path="recursos" element={<RecursosPage />} />
            <Route path="servicios" element={<ServiciosPage />} />
            <Route path="clientes" element={<ClientesPage />} />
            <Route path="turnos" element={<TurnosPage />} />
            <Route path="configuracion" element={<ConfigurationPage />} />
          </Route>

          <Route path="*" element={<div>Página no encontrada</div>} />
        </Routes>
      </BrowserRouter>
    </MantineProvider>
  );
}

// ESTA ES LA LÍNEA CLAVE QUE BUSCA VITE: "export default"
export default function App() {
  const [colorHexInicial, setColorHexInicial] = useState<string | null>(null);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    const cargarMarca = async () => {
      const rutasPublicas = ['/', '/login', '/registro'];
      if (rutasPublicas.includes(window.location.pathname)) {
        setCargando(false);
        return;
      }

      try {
        const config = await tenantService.getConfig();
        if (config.colorPrimario) {
          setColorHexInicial(config.colorPrimario);
        }
      } catch {
        console.warn('Usando color por defecto de turnifyTheme.');
      } finally {
        setCargando(false);
      }
    };

    cargarMarca();
  }, []);

  if (cargando) {
    return (
      <MantineProvider theme={turnifyTheme}>
        <Center h="100vh">
          <Loader type="dots" />
        </Center>
      </MantineProvider>
    );
  }

  return (
    <TenantThemeProvider initialColorHex={colorHexInicial}>
      <ThemedApp />
    </TenantThemeProvider>
  );
}