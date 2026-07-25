import httpClient from './httpClient';
import type { Rol, RolCreateDTO, RolUpdateDTO } from '../types/Rol';

export const rolesService = {
  getAll: async (): Promise<Rol[]> => {
    const response = await httpClient.get<Rol[]>('/roles');
    return response.data;
  },

  create: async (dto: RolCreateDTO): Promise<Rol> => {
    const response = await httpClient.post<Rol>('/roles', dto);
    return response.data;
  },

  update: async (id: number, dto: RolUpdateDTO): Promise<Rol> => {
    const response = await httpClient.put<Rol>(`/roles/${id}`, dto);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/roles/${id}`);
  },
};
