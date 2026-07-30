// src/types/Tenant.ts
import type { FrecuenciaLiquidacion } from './Liquidacion';

export interface TenantConfig {
  id?: number;
  nombre?: string;
  slug?: string;
  colorPrimario: string;
  permiteReservasPublicas: boolean;
  permiteSolapamiento?: boolean;
  frecuenciaLiquidacion: FrecuenciaLiquidacion;
}