import clienteHttpClient from './clienteHttpClient';
import type { Turno } from '../types/Turno';
import type { CrearTurnoPublicoRequest } from '../types/ReservaPublica';

export const misTurnosService = {
  getMisTurnos: async (): Promise<Turno[]> => {
    const response = await clienteHttpClient.get<Turno[]>('/mis-turnos');
    return response.data;
  },

  crear: async (dto: CrearTurnoPublicoRequest): Promise<Turno> => {
    const response = await clienteHttpClient.post<Turno>('/mis-turnos', dto);
    return response.data;
  },

  cancelar: async (id: number): Promise<void> => {
    await clienteHttpClient.delete(`/mis-turnos/${id}`);
  },
};
