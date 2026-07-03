import axios from 'axios';

const TENANT_ID = import.meta.env.VITE_TENANT_ID ?? '1';

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7162/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor: agrega el header en cada request, no hace falta repetirlo en cada servicio
httpClient.interceptors.request.use((config) => {
  config.headers['X-Tenant-Id'] = TENANT_ID;
  return config;
});

// Interceptor de respuesta: acá centralizamos el manejo de errores del ProblemDetails
// que devuelve tu GlobalExceptionHandler
httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      const problemDetails = error.response.data;
      console.error(`[API Error ${problemDetails.status}] ${problemDetails.title}: ${problemDetails.detail}`);
    } else {
      console.error('Error de red o servidor no disponible:', error.message);
    }
    return Promise.reject(error);
  }
);

export default httpClient;