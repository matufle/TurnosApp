import httpClient from './httpClient';
import type { Usuario, UsuarioCreateDTO, UsuarioUpdateDTO } from '../types/Usuario';

export const usuariosService = {
  getAll: async (): Promise<Usuario[]> => {
    const response = await httpClient.get<Usuario[]>('/usuarios');
    return response.data;
  },

  create: async (dto: UsuarioCreateDTO): Promise<Usuario> => {
    const response = await httpClient.post<Usuario>('/usuarios', dto);
    return response.data;
  },

  update: async (id: number, dto: UsuarioUpdateDTO): Promise<Usuario> => {
    const response = await httpClient.put<Usuario>(`/usuarios/${id}`, dto);
    return response.data;
  },

  activar: async (id: number): Promise<Usuario> => {
    const response = await httpClient.patch<Usuario>(`/usuarios/${id}/activar`);
    return response.data;
  },

  desactivar: async (id: number): Promise<Usuario> => {
    const response = await httpClient.patch<Usuario>(`/usuarios/${id}/desactivar`);
    return response.data;
  },
};
