// src/api/suscripcionService.ts
import httpClient from './httpClient';
import type { Suscripcion } from '../types/Suscripcion';

export const suscripcionService = {
  getEstado: async (): Promise<Suscripcion> => {
    const response = await httpClient.get('/suscripciones/estado');
    return response.data;
  },

  iniciar: async (): Promise<string> => {
    const response = await httpClient.post('/suscripciones/iniciar');
    return response.data.url;
  },

  cancelar: async (): Promise<void> => {
    await httpClient.post('/suscripciones/cancelar');
  },
};
