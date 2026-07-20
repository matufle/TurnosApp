// src/api/turnosService.ts
import httpClient from './httpClient';
import type { Turno, CrearTurnoDTO, CambiarEstadoTurnoDTO } from '../types/Turno';

export const turnosService = {
  getAll: async (): Promise<Turno[]> => {
    const response = await httpClient.get<Turno[]>('/turnos');
    return response.data;
  },

  crear: async (dto: CrearTurnoDTO): Promise<Turno> => {
    const response = await httpClient.post<Turno>('/turnos', dto);
    return response.data;
  },

  cancelar: async (id: number): Promise<void> => {
    await httpClient.delete(`/turnos/${id}`);
  },

  cambiarEstado: async (id: number, dto: CambiarEstadoTurnoDTO): Promise<Turno> => {
    const response = await httpClient.patch<Turno>(`/turnos/${id}/estado`, dto);
    return response.data;
  },
};