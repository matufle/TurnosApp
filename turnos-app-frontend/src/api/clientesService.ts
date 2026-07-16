// src/api/clientesService.ts
import httpClient from './httpClient';
import type { Cliente, CreateClienteDTO, UpdateClienteDTO } from '../types/Cliente';

export const clientesService = {
  getAll: async (): Promise<Cliente[]> => {
    const response = await httpClient.get<Cliente[]>('/clientes');
    return response.data;
  },

  create: async (dto: CreateClienteDTO): Promise<Cliente> => {
    const response = await httpClient.post<Cliente>('/clientes', dto);
    return response.data;
  },
  // En src/api/clientesService.ts
  update: async (id: number, dto: UpdateClienteDTO): Promise<Cliente> => {
    const response = await httpClient.put<Cliente>(`/clientes/${id}`, dto);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/clientes/${id}`);
  },
};

