// src/api/tenantService.ts
import httpClient from './httpClient';
import type { TenantConfig } from '../types/Tenant';
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