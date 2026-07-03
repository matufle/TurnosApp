// src/App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { LandingPage } from './pages/Landing/LandindPage';
import { PrivateLayout } from './layout/PrivateLayout';
import { ProtectedRoute } from './auth/ProtectedRoute';
//import { TurnosPage } from './pages/Turnos/TurnosPage';
//import { RecursosPage } from './pages/Recursos/RecursosPage';
//import { ServiciosPage } from './pages/Servicios/ServiciosPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Pública */}
        <Route path="/" element={<LandingPage />} />

        {/* Privada, protegida y con layout compartido */}
        <Route
          path="/app"
          element={
            <ProtectedRoute>
              <PrivateLayout />
            </ProtectedRoute>
          }
        >
          {/* Aquí van las rutas hijas de la sección privada */}
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;