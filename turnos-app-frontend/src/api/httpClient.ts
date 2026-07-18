// src/api/httpClient.ts
import axios from 'axios';
import { API_BASE_URL } from '../config/runtimeConfig';

const httpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

httpClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('turnify_token');
  const tenantId = localStorage.getItem('turnify_tenant_id');

  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  if (tenantId) {
    config.headers['X-Tenant-Id'] = tenantId;
  }

  return config;
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token vencido o inválido: limpiamos sesión y mandamos a login
      localStorage.removeItem('turnify_token');
      localStorage.removeItem('turnify_tenant_id');
      window.location.href = '/login';
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

export default httpClient;