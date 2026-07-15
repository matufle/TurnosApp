// src/api/recursosService.ts
import httpClient from './httpClient';
import type { Recurso, RecursoCreateDTO } from '../types/Recurso';

export const recursosService = {
  getAll: async (): Promise<Recurso[]> => {
    const response = await httpClient.get<Recurso[]>('/recursos');
    return response.data;
  },

  getById: async (id: number): Promise<Recurso> => {
    const response = await httpClient.get<Recurso>(`/recursos/${id}`);
    return response.data;
  },

  create: async (dto: RecursoCreateDTO): Promise<Recurso> => {
    const response = await httpClient.post<Recurso>('/recursos', dto);
    return response.data;
  },
  
  update: async (id: number, data: RecursoCreateDTO) => {
    // CAMBIO ACÁ: Usamos httpClient y le sacamos el /api/
    const response = await httpClient.put(`/recursos/${id}`, data);
    return response.data;
  }
};