// src/types/Caja.ts

// Coincide con los enums del backend (src/Core.Domain/Enums/EstadoSesionCaja.cs, TipoMovimientoCaja.cs)
export type EstadoSesionCaja = 'Abierta' | 'Cerrada';
export type TipoMovimientoCaja = 'Ingreso' | 'Egreso';

export interface MovimientoCaja {
  id: number;
  tipo: TipoMovimientoCaja;
  monto: number;
  metodoPagoId: number | null;
  nombreMetodoPagoSnapshot: string;
  esEfectivoSnapshot: boolean;
  concepto: string;
  fechaHora: string;
  usuarioId: number;
  usuarioNombre: string;
  cobroId: number | null;
  movimientoOrigenId: number | null;
}

// Solo Efectivo tiene sentido de "declarado/esperado/diferencia" — el resto de los
// medios de pago se muestra únicamente como total facturado (para conciliar después).
export interface DesgloseMedioPago {
  metodoPagoId: number | null;
  nombre: string;
  esEfectivo: boolean;
  totalIngresos: number;
  totalEgresos: number;
  total: number;
}

export interface SesionCaja {
  id: number;
  fechaApertura: string;
  fechaCierre: string | null;
  montoInicial: number;
  montoFinalDeclarado: number | null;
  montoEsperadoEfectivo: number;
  diferencia: number | null;
  estado: EstadoSesionCaja;
  cierreForzado: boolean;
  observaciones: string | null;
  usuarioAperturaId: number;
  usuarioAperturaNombre: string;
  usuarioCierreId: number | null;
  usuarioCierreNombre: string | null;
  movimientos: MovimientoCaja[];
  desglosePorMedioPago: DesgloseMedioPago[];
}

// Versión liviana para el historial paginado, sin el detalle de movimientos.
export interface SesionCajaListItem {
  id: number;
  fechaApertura: string;
  fechaCierre: string | null;
  montoInicial: number;
  montoFinalDeclarado: number | null;
  montoEsperadoEfectivo: number;
  diferencia: number | null;
  cierreForzado: boolean;
  usuarioAperturaId: number;
  usuarioAperturaNombre: string;
  usuarioCierreId: number | null;
  usuarioCierreNombre: string | null;
}

export interface HistorialSesionesCaja {
  items: SesionCajaListItem[];
  totalCount: number;
  pagina: number;
  tamanoPagina: number;
}

export interface AbrirSesionCajaDTO {
  montoInicial: number;
  observaciones?: string;
}

export interface RegistrarMovimientoCajaDTO {
  tipo: TipoMovimientoCaja;
  monto: number;
  metodoPagoId: number;
  concepto: string;
}

export interface CerrarSesionCajaDTO {
  montoFinalDeclarado: number;
  observaciones?: string;
}

export interface HistorialSesionesCajaFiltro {
  fechaDesde?: string;
  fechaHasta?: string;
  pagina?: number;
  tamanoPagina?: number;
}
