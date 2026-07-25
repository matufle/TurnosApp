// src/types/Cobro.ts
import type { TipoModificadorPago } from './MetodoPago';

export interface Cobro {
  id: number;
  turnoId: number;
  metodoPagoId: number | null;
  nombreMetodoPagoSnapshot: string;
  tipoModificadorSnapshot: TipoModificadorPago;
  porcentajeModificadorSnapshot: number;
  porcentajeComisionSnapshot: number;
  precioBase: number;
  montoModificadorCliente: number;
  precioFinal: number;
  montoComision: number;
  gananciaNeta: number;
  creadoEn: string;
  creadoPor: string | null;
  modificadoEn: string | null;
  modificadoPor: string | null;
}

export interface CobroCreateDTO {
  turnoId: number;
  metodoPagoId: number;
  precioBase: number;
}

export interface CobroUpdateDTO {
  metodoPagoId: number;
  precioBase: number;
}

export interface CobroListItem {
  id: number;
  turnoId: number;
  clienteNombreCompleto: string;
  serviciosResumen: string;
  fechaHoraTurno: string;
  metodoPagoId: number | null;
  nombreMetodoPagoSnapshot: string;
  tipoModificadorSnapshot: TipoModificadorPago;
  porcentajeModificadorSnapshot: number;
  precioBase: number;
  montoModificadorCliente: number;
  precioFinal: number;
  montoComision: number;
  gananciaNeta: number;
  creadoEn: string;
}

export interface HistorialCobrosFiltro {
  fechaDesde?: string;
  fechaHasta?: string;
  busqueda?: string;
  pagina?: number;
  tamanoPagina?: number;
}

export interface HistorialCobros {
  items: CobroListItem[];
  totalCount: number;
  pagina: number;
  tamanoPagina: number;
  totalCobradoPeriodo: number;
  comisionesTotalesPeriodo: number;
  saldoPendienteGlobal: number;
}
