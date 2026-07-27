// src/api/clienteHttpClient.ts
// Instancia de axios separada de httpClient.ts: identidad de cliente self-service,
// namespace propio de localStorage, para que una sesión de cliente y una de staff
// convivan en el mismo navegador sin pisarse (ni compartir el 401-handler).
import axios from 'axios';
import { API_BASE_URL } from '../config/runtimeConfig';

const clienteHttpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

clienteHttpClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('turnify_cliente_token');
  const tenantId = localStorage.getItem('turnify_cliente_tenant_id');

  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  if (tenantId) {
    config.headers['X-Tenant-Id'] = tenantId;
  }

  return config;
});

clienteHttpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const slug = localStorage.getItem('turnify_cliente_slug');
      localStorage.removeItem('turnify_cliente_token');
      localStorage.removeItem('turnify_cliente_tenant_id');
      localStorage.removeItem('turnify_cliente_slug');
      if (slug) {
        window.location.href = `/reservas/${slug}/login`;
      }
    }

    if (error.response) {
      const problemDetails = error.response.data;
      console.error(`[API Error ${problemDetails.status}] ${problemDetails.title}: ${problemDetails.detail}`);
    } else {
      console.error('Error de red o servidor no disponible:', error.message);
    }

    return Promise.reject(error);
  }
);

export default clienteHttpClient;
