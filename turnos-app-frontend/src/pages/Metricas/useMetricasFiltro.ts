// src/pages/Metricas/useMetricasFiltro.ts
import { useMemo, useState } from 'react';
import { format, startOfMonth, startOfQuarter, startOfWeek, startOfYear } from 'date-fns';

export type PresetRango = 'hoy' | 'semana' | 'mes' | 'trimestre' | 'anio' | 'personalizado';

// DatePickerInput entrega/recibe strings "YYYY-MM-DD" (no Date) — se mantienen como
// string de punta a punta. Convertirlos a Date acá dispararía el bug clásico de
// `new Date("YYYY-MM-DD")` (se interpreta como medianoche UTC), que corre el día
// mostrado hacia atrás en husos horarios negativos como Argentina (UTC-3).
// Mismo patrón que HistorialCobrosPage.tsx.
export function finDeDia(fecha: string): string {
  return `${fecha}T23:59:59.999`;
}

function hoyStr(): string {
  return format(new Date(), 'yyyy-MM-dd');
}

function calcularDesde(preset: PresetRango): string {
  const hoy = new Date();
  switch (preset) {
    case 'hoy':
      return format(hoy, 'yyyy-MM-dd');
    case 'semana':
      return format(startOfWeek(hoy, { weekStartsOn: 1 }), 'yyyy-MM-dd');
    case 'trimestre':
      return format(startOfQuarter(hoy), 'yyyy-MM-dd');
    case 'anio':
      return format(startOfYear(hoy), 'yyyy-MM-dd');
    case 'mes':
    default:
      return format(startOfMonth(hoy), 'yyyy-MM-dd');
  }
}

export function useMetricasFiltro() {
  const [preset, setPresetState] = useState<PresetRango>('mes');
  const [fechaDesde, setFechaDesde] = useState<string | null>(calcularDesde('mes'));
  const [fechaHasta, setFechaHasta] = useState<string | null>(hoyStr());

  function setPreset(nuevoPreset: PresetRango) {
    setPresetState(nuevoPreset);
    if (nuevoPreset !== 'personalizado') {
      setFechaDesde(calcularDesde(nuevoPreset));
      setFechaHasta(hoyStr());
    }
  }

  function setRangoPersonalizado(desde: string | null, hasta: string | null) {
    setPresetState('personalizado');
    setFechaDesde(desde);
    setFechaHasta(hasta);
  }

  // Filtro base (fechas) compartido por las 5 pestañas; cada tab le suma sus propios
  // filtros de entidad (recursoId, metodoPagoId, etc.) antes de llamar a metricasService.
  const filtroBase = useMemo(
    () => ({
      fechaDesde: fechaDesde ?? undefined,
      fechaHasta: fechaHasta ? finDeDia(fechaHasta) : undefined,
    }),
    [fechaDesde, fechaHasta]
  );

  return {
    preset,
    fechaDesde,
    fechaHasta,
    setPreset,
    setRangoPersonalizado,
    filtroBase,
  };
}
