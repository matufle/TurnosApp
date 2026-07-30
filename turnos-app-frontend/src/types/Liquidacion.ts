// src/types/Liquidacion.ts

// Coincide con los enums del backend (src/Core.Domain/Enums/TipoComision.cs,
// EstadoLiquidacion.cs, FrecuenciaLiquidacion.cs)
export type TipoComision = 'Porcentaje' | 'MontoFijo';
export type EstadoLiquidacion = 'Generada' | 'Pagada' | 'Anulada';
export type FrecuenciaLiquidacion = 'Semanal' | 'Quincenal' | 'Mensual';

export interface ReglaComision {
  id: number;
  recursoId: number;
  servicioId: number | null;
  servicioNombre: string | null;
  tipo: TipoComision;
  valor: number;
  activo: boolean;
}

export interface ReglaComisionCreateDTO {
  recursoId: number;
  servicioId: number | null;
  tipo: TipoComision;
  valor: number;
}

export interface ReglaComisionUpdateDTO {
  tipo: TipoComision;
  valor: number;
  activo: boolean;
}

export interface AdelantoProfesional {
  id: number;
  recursoId: number;
  monto: number;
  fecha: string;
  concepto: string | null;
  liquidacionId: number | null;
}

export interface AdelantoProfesionalCreateDTO {
  recursoId: number;
  monto: number;
  fecha: string;
  concepto?: string;
}

export interface LiquidacionDetalle {
  id: number;
  turnoId: number;
  turnoFecha: string;
  servicioId: number;
  servicioNombre: string;
  precioBaseAplicado: number;
  tipoComisionSnapshot: TipoComision;
  valorComisionSnapshot: number;
  montoComisionCalculado: number;
}

export interface LiquidacionListItem {
  id: number;
  recursoId: number;
  recursoNombre: string;
  periodoDesde: string;
  periodoHasta: string;
  fechaGeneracion: string;
  estado: EstadoLiquidacion;
  montoBrutoComision: number;
  montoAdelantos: number;
  montoNeto: number;
}

export interface Liquidacion {
  id: number;
  recursoId: number;
  recursoNombre: string;
  periodoDesde: string;
  periodoHasta: string;
  fechaGeneracion: string;
  estado: EstadoLiquidacion;
  fechaPago: string | null;
  usuarioPagoId: number | null;
  usuarioPagoNombre: string | null;
  observaciones: string | null;
  montoBrutoComision: number;
  montoAdelantos: number;
  montoNeto: number;
  detalles: LiquidacionDetalle[];
  adelantos: AdelantoProfesional[];
}

export interface MarcarPagadaLiquidacionDTO {
  observaciones?: string;
}

export interface AnularLiquidacionDTO {
  observaciones?: string;
}
