// src/App.tsx
import { lazy, Suspense, useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { MantineProvider, createTheme, Loader, Center } from '@mantine/core';

import { theme as turnifyTheme } from './theme/turnifyTheme';
import { generateShades } from './theme/generateShades';
import { applyTailwindBrandColor } from './theme/applyTailwindBrandColor';
import { TenantThemeProvider } from './context/TenantThemeContext';
import { useTenantTheme } from './context/useTenantTheme';
import { AuthProvider } from './context/AuthContext';
import { tenantService } from './api/tenantService';

import { LandingPage } from './pages/Landing/LandindPage';
import { LoginPage } from './pages/Login/LoginPage';
import { RegisterPage } from './pages/Register/RegisterPage';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { ClienteProtectedRoute } from './auth/ClienteProtectedRoute';

// Todo lo que vive detrás de un login (staff o cliente) se carga de forma perezosa:
// un visitante anónimo que solo ve la Landing no debería pagar el costo de descargar
// el JS de Métricas (@mantine/charts), Turnos (react-big-calendar) ni del resto del panel.
const DashboardLayout = lazy(() => import('./layout/DashboardLayout').then((m) => ({ default: m.DashboardLayout })));
const DashboardPage = lazy(() => import('./pages/Dashboard/DashboardPage').then((m) => ({ default: m.DashboardPage })));
const RecursosPage = lazy(() => import('./pages/Recursos/RecursosPage').then((m) => ({ default: m.RecursosPage })));
const HorariosPage = lazy(() => import('./pages/Recursos/HorariosPage').then((m) => ({ default: m.HorariosPage })));
const ServiciosPage = lazy(() => import('./pages/Servicios/ServiciosPage').then((m) => ({ default: m.ServiciosPage })));
const TurnosPage = lazy(() => import('./pages/Turnos/TurnosPage').then((m) => ({ default: m.TurnosPage })));
const ClientesPage = lazy(() => import('./pages/Clientes/ClientesPage').then((m) => ({ default: m.ClientesPage })));
const ConfigurationPage = lazy(() => import('./pages/Configuration/ConfigurationPage').then((m) => ({ default: m.ConfigurationPage })));
const MetodosPagoPage = lazy(() => import('./pages/MetodosPago/MetodosPagoPage').then((m) => ({ default: m.MetodosPagoPage })));
const HistorialCobrosPage = lazy(() => import('./pages/Cobros/HistorialCobrosPage').then((m) => ({ default: m.HistorialCobrosPage })));
const UsuariosPage = lazy(() => import('./pages/Usuarios/UsuariosPage').then((m) => ({ default: m.UsuariosPage })));
const RolesPage = lazy(() => import('./pages/Roles/RolesPage').then((m) => ({ default: m.RolesPage })));
const MetricasPage = lazy(() => import('./pages/Metricas/MetricasPage').then((m) => ({ default: m.MetricasPage })));
const ListaEsperaPage = lazy(() => import('./pages/ListaEspera/ListaEsperaPage').then((m) => ({ default: m.ListaEsperaPage })));
const CajaPage = lazy(() => import('./pages/Caja/CajaPage').then((m) => ({ default: m.CajaPage })));
const ReservaTenantLayout = lazy(() => import('./pages/Reservas/ReservaTenantLayout').then((m) => ({ default: m.ReservaTenantLayout })));
const LoginClientePage = lazy(() => import('./pages/Reservas/LoginClientePage').then((m) => ({ default: m.LoginClientePage })));
const RegistroClientePage = lazy(() => import('./pages/Reservas/RegistroClientePage').then((m) => ({ default: m.RegistroClientePage })));
const MisTurnosPage = lazy(() => import('./pages/Reservas/MisTurnosPage').then((m) => ({ default: m.MisTurnosPage })));
const CatalogoPage = lazy(() => import('./pages/Reservas/CatalogoPage').then((m) => ({ default: m.CatalogoPage })));
const ReservarPage = lazy(() => import('./pages/Reservas/ReservarPage').then((m) => ({ default: m.ReservarPage })));

function RouteFallback() {
  return (
    <Center h="100vh">
      <Loader type="dots" color="cyan" />
    </Center>
  );
}

function ThemedApp() {
  const { colorHex } = useTenantTheme();

  useEffect(() => {
    applyTailwindBrandColor(colorHex && colorHex !== '#0EA5E9' ? colorHex : null);
  }, [colorHex]);

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
        <Suspense fallback={<RouteFallback />}>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/registro" element={<RegisterPage />} />

          {/* Self-service de cara al cliente final: identidad y branding propios,
              completamente separados del staff (ver ClienteAuthProvider). */}
          <Route path="/reservas/:slug" element={<ReservaTenantLayout />}>
            <Route index element={<CatalogoPage />} />
            <Route path="login" element={<LoginClientePage />} />
            <Route path="registro" element={<RegistroClientePage />} />
            <Route
              path="reservar"
              element={
                <ClienteProtectedRoute>
                  <ReservarPage />
                </ClienteProtectedRoute>
              }
            />
            <Route
              path="mis-turnos"
              element={
                <ClienteProtectedRoute>
                  <MisTurnosPage />
                </ClienteProtectedRoute>
              }
            />
          </Route>

          <Route
            path="/app"
            element={
              <ProtectedRoute>
                <DashboardLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<DashboardPage />} />
            <Route path="recursos" element={<RecursosPage />} />
            <Route
              path="recursos/horarios"
              element={
                <ProtectedRoute permiso="GestionarRecursos">
                  <HorariosPage />
                </ProtectedRoute>
              }
            />
            <Route path="servicios" element={<ServiciosPage />} />
            <Route path="clientes" element={<ClientesPage />} />
            <Route path="turnos" element={<TurnosPage />} />
            <Route path="lista-espera" element={<ListaEsperaPage />} />
            <Route
              path="metodos-pago"
              element={
                <ProtectedRoute permiso="GestionarMetodosPago">
                  <MetodosPagoPage />
                </ProtectedRoute>
              }
            />
            <Route path="cobros" element={<HistorialCobrosPage />} />
            <Route
              path="caja"
              element={
                <ProtectedRoute permiso={['VerCaja', 'GestionarCaja']}>
                  <CajaPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="metricas"
              element={
                <ProtectedRoute permiso="VerReportes">
                  <MetricasPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="configuracion"
              element={
                <ProtectedRoute permiso="GestionarConfiguracionNegocio">
                  <ConfigurationPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="usuarios"
              element={
                <ProtectedRoute permiso="GestionarUsuarios">
                  <UsuariosPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="roles"
              element={
                <ProtectedRoute permiso="GestionarRoles">
                  <RolesPage />
                </ProtectedRoute>
              }
            />
          </Route>

          <Route path="*" element={<div>Página no encontrada</div>} />
        </Routes>
        </Suspense>
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
      const esRutaPublica =
        rutasPublicas.includes(window.location.pathname) ||
        window.location.pathname.startsWith('/reservas/');

      if (esRutaPublica) {
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
      <AuthProvider>
        <ThemedApp />
      </AuthProvider>
    </TenantThemeProvider>
  );
}