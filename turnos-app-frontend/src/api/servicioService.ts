// src/api/serviciosService.ts
import httpClient from './httpClient';
import type{ Servicio, ServicioCreateDTO } from '../types/Servicio';

export const serviciosService = {
  getAll: async (): Promise<Servicio[]> => {
    const response = await httpClient.get<Servicio[]>('/servicios');
    return response.data;
  },

  create: async (dto: ServicioCreateDTO): Promise<Servicio> => {
    const response = await httpClient.post<Servicio>('/servicios', dto);
    return response.data;
  },
};