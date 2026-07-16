// src/types/Tenant.ts
export interface TenantConfig {
  id?: number;
  nombre?: string;
  colorPrimario: string;
  permiteReservasPublicas: boolean;
  permiteSolapamiento?: boolean;
}