// src/api/clientesService.ts
import httpClient from './httpClient';
import type { Cliente, CreateClienteDTO } from '../types/Cliente';

export const clientesService = {
  getAll: async (): Promise<Cliente[]> => {
    const response = await httpClient.get<Cliente[]>('/clientes');
    return response.data;
  },

  create: async (dto: CreateClienteDTO): Promise<Cliente> => {
    const response = await httpClient.post<Cliente>('/clientes', dto);
    return response.data;
  },
};