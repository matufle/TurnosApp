import { useEffect, useState } from 'react';
import { recursosService } from './api/recursosService';
import type { Recurso } from './types/Recurso';

function App() {
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    // Esta función se ejecuta apenas carga la página
    const cargarRecursos = async () => {
      try {
        const data = await recursosService.getAll();
        console.log("¡Datos recibidos del backend!", data);
        setRecursos(data);
      } catch (err) {
        console.error("Error al traer recursos:", err);
        setError('Falló la conexión. Mirá la consola (F12).');
      }
    };

    cargarRecursos();
  }, []);

  return (
    <div style={{ padding: '20px', fontFamily: 'sans-serif' }}>
      <h1>Prueba de Conexión Backend 🚀</h1>
      
      {error && <p style={{ color: 'red' }}>{error}</p>}
      
      <h2>Mis Recursos (Tenant 1):</h2>
      <ul>
        {recursos.length === 0 && !error ? <li>Cargando o no hay recursos...</li> : null}
        
        {recursos.map((recurso) => (
          <li key={recurso.id}>
            <strong>ID:</strong> {recurso.id} | <strong>Nombre:</strong> {recurso.nombre}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;