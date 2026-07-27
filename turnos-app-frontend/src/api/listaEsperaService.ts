// src/api/listaEsperaService.ts
import httpClient from './httpClient';
import type { ListaEsperaEntry, CrearListaEsperaDTO } from '../types/ListaEspera';

export const listaEsperaService = {
  getAll: async (): Promise<ListaEsperaEntry[]> => {
    const response = await httpClient.get<ListaEsperaEntry[]>('/listaespera');
    return response.data;
  },

  crear: async (dto: CrearListaEsperaDTO): Promise<ListaEsperaEntry> => {
    const response = await httpClient.post<ListaEsperaEntry>('/listaespera', dto);
    return response.data;
  },

  cancelar: async (id: number): Promise<ListaEsperaEntry> => {
    const response = await httpClient.patch<ListaEsperaEntry>(`/listaespera/${id}/cancelar`);
    return response.data;
  },
};
