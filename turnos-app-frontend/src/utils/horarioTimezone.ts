// src/utils/horarioTimezone.ts
// HorarioAtencion.HoraInicio/HoraFin (y los slots que devuelve /disponibilidad) usan la misma
// convención que Turno.FechaHoraInicio en TurnosPage.tsx: son horas "UTC-equivalentes", no
// hora local — la conversión sucede acá, en el browser, nunca en el backend (mismo motivo que
// combinarFechaYHora). Se asume que el navegador corre en la misma zona horaria que el negocio
// (única zona horaria soportada hoy — ver el gotcha de timezones documentado en CLAUDE.md).

export function horaLocalAUtc(horaLocal: string): string {
  const [h, m] = horaLocal.split(':').map(Number);
  const d = new Date();
  d.setHours(h, m, 0, 0);
  return `${String(d.getUTCHours()).padStart(2, '0')}:${String(d.getUTCMinutes()).padStart(2, '0')}`;
}

export function horaUtcALocal(horaUtc: string): string {
  const [h, m] = horaUtc.split(':').map(Number);
  const d = new Date();
  d.setUTCHours(h, m, 0, 0);
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
}
