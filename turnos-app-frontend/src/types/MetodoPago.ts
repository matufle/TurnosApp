// src/types/MetodoPago.ts

// Coincide con el enum TipoModificadorPago del backend (src/Core.Domain/Enums/TipoModificadorPago.cs)
export type TipoModificadorPago = 'Ninguno' | 'Bonificacion' | 'Recargo';

export interface MetodoPago {
  id: number;
  nombre: string;
  tipoModificador: TipoModificadorPago;
  porcentajeModificador: number;
  porcentajeComision: number;
  activo: boolean;
  esEfectivo: boolean;
}

export interface MetodoPagoCreateDTO {
  nombre: string;
  tipoModificador: TipoModificadorPago;
  porcentajeModificador: number;
  porcentajeComision: number;
  esEfectivo: boolean;
}

export interface MetodoPagoUpdateDTO extends MetodoPagoCreateDTO {
  activo: boolean;
}
