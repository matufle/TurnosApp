import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { LandingPage } from './pages/Landing/LandindPage';
import { DashboardLayout } from './layout/DashboardLayout';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { LoginPage } from './pages/Login/LoginPage';
import { RegisterPage } from './pages/Register/RegisterPage';
import { RecursosPage } from './pages/Recursos/RecursosPage';
import { ServiciosPage } from './pages/Servicios/ServiciosPage';

function App() {
  return (
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
        </Route>

        {/* Catch-all — evita pantallas en blanco silenciosas como la de /register vs /registro */}
        <Route path="*" element={<div>Página no encontrada</div>} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;