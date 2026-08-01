// src/types/Suscripcion.ts
export type EstadoSuscripcion = 'Trial' | 'Activa' | 'PastDue' | 'Cancelada';

export interface Suscripcion {
  estadoSuscripcion: EstadoSuscripcion;
  suscripcionVenceEn: string | null;
  esGrandfathered: boolean;
  planNombre: string | null;
  planPrecioMensual: number | null;
  tieneSuscripcionIniciada: boolean;
}
