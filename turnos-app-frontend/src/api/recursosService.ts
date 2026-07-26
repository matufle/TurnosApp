// src/api/recursosService.ts
import httpClient from './httpClient';
import type { Recurso, RecursoCreateDTO, RecursoUpdateDTO, UsuarioParaVincular } from '../types/Recurso';

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

  update: async (id: number, data: RecursoUpdateDTO): Promise<Recurso> => {
    const response = await httpClient.put<Recurso>(`/recursos/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/recursos/${id}`);
  },

  getUsuariosDisponibles: async (recursoIdActual?: number): Promise<UsuarioParaVincular[]> => {
    const response = await httpClient.get<UsuarioParaVincular[]>('/recursos/usuarios-disponibles', {
      params: recursoIdActual ? { recursoIdActual } : undefined,
    });
    return response.data;
  },
};