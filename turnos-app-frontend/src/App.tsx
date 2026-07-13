// App.tsx corregido
import { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { MantineProvider, createTheme, Loader, Center } from '@mantine/core';

// 1. Tema base
import { theme as turnifyTheme } from './theme/turnifyTheme';
import { generateShades } from './theme/generateShades';

// Servicios
import { tenantService } from './api/tenantService';

// Páginas y Layouts
import { LandingPage } from './pages/Landing/LandindPage';
import { DashboardLayout } from './layout/DashboardLayout';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { LoginPage } from './pages/Login/LoginPage';
import { RegisterPage } from './pages/Register/RegisterPage';
import { RecursosPage } from './pages/Recursos/RecursosPage';
import { ServiciosPage } from './pages/Servicios/ServiciosPage';
import { TurnosPage } from './pages/Turnos/TurnosPage';
import { ClientesPage } from './pages/Clientes/ClientesPage';
import { ConfigurationPage } from './pages/Configuration/ConfigurationPage.tsx';

export default function App() {
  const [colorHex, setColorHex] = useState<string | null>(null);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    const cargarMarca = async () => {
      try {
        const config = await tenantService.getConfig();
        if (config.colorPrimario) {
          setColorHex(config.colorPrimario);
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

  // Si hay color dinámico, lo inyectamos EN LA MISMA clave 'cyan' del theme base.
  // Así, todo componente con color="cyan" hardcodeado (botones, badges, el
  // eventPropGetter del calendario en TurnosPage, etc.) automáticamente
  // usa el color del tenant sin tener que salir a buscar y reemplazar cada uso.
  const finalTheme =
    colorHex && colorHex !== '#0EA5E9'
      ? createTheme({
          ...turnifyTheme,
          colors: {
            ...turnifyTheme.colors,
            cyan: generateShades(colorHex),
          },
          autoContrast: true,
          luminanceThreshold: 0.45,
        })
      : turnifyTheme;

  return (
    // key fuerza que Mantine recalcule todas las CSS variables desde cero
    // cuando cambia colorHex, evitando el bug de "color ignorado del todo".
    <MantineProvider theme={finalTheme} key={colorHex ?? 'default'}>
      <BrowserRouter>
        <Routes>
          {/* Pública */}
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/registro" element={<RegisterPage />} />

          {/* Privada, protegida y con layout compartido */}
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

          {/* Catch-all */}
          <Route path="*" element={<div>Página no encontrada</div>} />
        </Routes>
      </BrowserRouter>
    </MantineProvider>
  );
}