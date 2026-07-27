// Coincide con el enum DayOfWeek de .NET: Sunday=0 ... Saturday=6.
export type DiaSemana = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export const NOMBRES_DIAS: Record<DiaSemana, string> = {
  0: 'Domingo',
  1: 'Lunes',
  2: 'Martes',
  3: 'Miércoles',
  4: 'Jueves',
  5: 'Viernes',
  6: 'Sábado',
};

export interface HorarioAtencion {
  id: number;
  diaSemana: DiaSemana;
  horaInicio: string; // "HH:mm:ss", UTC-equivalente (ver src/utils/horarioTimezone.ts)
  horaFin: string;
}

export interface HorarioAtencionItem {
  diaSemana: DiaSemana;
  horaInicio: string;
  horaFin: string;
}
