// src/api/tenantService.ts
import httpClient from './httpClient';

export interface TenantConfig {
  id?: number;
  nombre?: string;
  colorPrimario: string;
  permiteReservasPublicas: boolean;
  permitirSolapamiento?: boolean;
}

export const tenantService = {
  getConfig: async (): Promise<TenantConfig> => {
    // Usamos httpClient y sacamos el /api/
    const response = await httpClient.get('/tenants/config'); 
    return response.data;
  },

  updateConfig: async (data: Partial<TenantConfig>): Promise<void> => {
    // Usamos httpClient y sacamos el /api/
    await httpClient.put('/tenants/config', data); 
  }
};