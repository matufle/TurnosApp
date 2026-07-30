// src/api/cajaService.ts
import httpClient from './httpClient';
import type {
  SesionCaja,
  MovimientoCaja,
  HistorialSesionesCaja,
  AbrirSesionCajaDTO,
  RegistrarMovimientoCajaDTO,
  CerrarSesionCajaDTO,
  HistorialSesionesCajaFiltro,
} from '../types/Caja';

export const cajaService = {
  getSesionAbierta: async (): Promise<SesionCaja | null> => {
    // Sin sesión abierta el backend responde 204 No Content — axios lo entrega como
    // string vacío, no null, así que lo normalizamos acá.
    const response = await httpClient.get<SesionCaja | null>('/caja/sesion-abierta');
    return response.data || null;
  },

  getById: async (id: number): Promise<SesionCaja> => {
    const response = await httpClient.get<SesionCaja>(`/caja/sesiones/${id}`);
    return response.data;
  },

  abrir: async (dto: AbrirSesionCajaDTO): Promise<SesionCaja> => {
    const response = await httpClient.post<SesionCaja>('/caja/sesiones', dto);
    return response.data;
  },

  registrarMovimiento: async (dto: RegistrarMovimientoCajaDTO): Promise<MovimientoCaja> => {
    const response = await httpClient.post<MovimientoCaja>('/caja/movimientos', dto);
    return response.data;
  },

  cerrar: async (id: number, dto: CerrarSesionCajaDTO): Promise<SesionCaja> => {
    const response = await httpClient.patch<SesionCaja>(`/caja/sesiones/${id}/cerrar`, dto);
    return response.data;
  },

  getHistorial: async (filtro: HistorialSesionesCajaFiltro): Promise<HistorialSesionesCaja> => {
    const response = await httpClient.get<HistorialSesionesCaja>('/caja/sesiones/historial', { params: filtro });
    return response.data;
  },
};
