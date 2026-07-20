// src/pages/Turnos/TurnosPage.tsx
import { useEffect, useMemo, useState } from 'react';
import axios from 'axios';
import {
  Button,
  Modal,
  Drawer,
  TextInput,
  Select,
  MultiSelect,
  Stack,
  Text,
  Switch,
  Badge,
  ColorSwatch,
  Divider,
  Group,
  Alert,
} from '@mantine/core';
import { DatePickerInput, TimeInput } from '@mantine/dates';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconAlertCircle, IconUser, IconMapPin, IconBriefcase, IconCheck, IconClock, IconCalendarEvent } from '@tabler/icons-react';
import { Calendar, dateFnsLocalizer } from 'react-big-calendar';
import type { SlotInfo, Event as CalendarEvent, ToolbarProps } from 'react-big-calendar';
import { format } from 'date-fns/format';
import { parse } from 'date-fns/parse';
import { startOfWeek } from 'date-fns/startOfWeek';
import { getDay } from 'date-fns/getDay';
import { startOfMonth } from 'date-fns/startOfMonth';
import { endOfMonth } from 'date-fns/endOfMonth';
import { endOfWeek } from 'date-fns/endOfWeek';
import { addDays } from 'date-fns/addDays';
import { addMonths } from 'date-fns/addMonths';
import { subMonths } from 'date-fns/subMonths';
import { isSameMonth } from 'date-fns/isSameMonth';
import { isSameDay } from 'date-fns/isSameDay';
import { es } from 'date-fns/locale/es';

import '@mantine/dates/styles.css';
import 'react-big-calendar/lib/css/react-big-calendar.css';

import { turnosService } from '../../api/turnosService';
import { clientesService } from '../../api/clientesService';
import { recursosService } from '../../api/recursosService';
import { serviciosService } from '../../api/servicioService';
import type { Turno, EstadoTurnoEditable } from '../../types/Turno';
import type { Cliente } from '../../types/Cliente';
import type { Recurso } from '../../types/Recurso';
import type { Servicio } from '../../types/Servicio';
import { getContrastTextColor } from '../../utils/colorContrast.ts';

const localizer = dateFnsLocalizer({
  format,
  parse,
  startOfWeek: () => startOfWeek(new Date(), { locale: es }),
  getDay,
  locales: { es },
});

const VISTAS = [
  { value: 'day', label: 'Día' },
  { value: 'week', label: 'Semana' },
  { value: 'month', label: 'Mes' },
] as const;

// Estados que se pueden asignar manualmente. Cancelado queda afuera:
// para eso está la acción dedicada de "Cancelar turno".
const ESTADOS_EDITABLES: { value: EstadoTurnoEditable; label: string }[] = [
  { value: 'Pendiente', label: 'Pendiente' },
  { value: 'Confirmado', label: 'Confirmado' },
  { value: 'EnCurso', label: 'En curso' },
  { value: 'Completado', label: 'Completado' },
  { value: 'Ausente', label: 'Ausente (no se presentó)' },
];

const COLOR_POR_ESTADO: Record<string, string> = {
  Pendiente: 'yellow',
  Confirmado: 'blue',
  EnCurso: 'grape',
  Completado: 'green',
  Ausente: 'orange',
  Cancelado: 'red',
};

// Un turno "vencido" es uno cuya hora de fin ya pasó pero sigue en un estado
// no definitivo (nadie lo marcó como Completado/Ausente ni se canceló).
// Es un cálculo al vuelo, no un estado que se persista en la base.
const ESTADOS_FINALES = new Set(['Completado', 'Ausente', 'Cancelado']);

function esTurnoVencido(turno: Turno, ahora: Date): boolean {
  return !ESTADOS_FINALES.has(turno.estado) && new Date(turno.fechaHoraFin) < ahora;
}

interface TurnoFormValues {
  esClienteNuevo: boolean;
  clienteId: string | null;
  clienteNombre: string;
  clienteApellido: string;
  clienteTelefono: string;
  recursoId: string | null;
  servicioIds: string[];
  fecha: Date | string | null;
  hora: string;
}

interface TurnoCalendarEvent extends CalendarEvent {
  resource: Turno;
  colorRecurso: string;
  recursoId: number;
}

function combinarFechaYHora(fecha: Date | string, hora: string): Date {
  const [horas, minutos] = hora.split(':').map(Number);

  let anio: number;
  let mes: number;
  let dia: number;

  if (typeof fecha === 'string') {
    // DatePickerInput entrega "YYYY-MM-DD" (fecha local, sin hora). new Date(string)
    // NO sirve acá: un string solo-fecha lo interpreta como medianoche UTC, y eso
    // corre el día para atrás en husos detrás de UTC (ej: Argentina) al combinarlo
    // con una hora local. Por eso parseamos los componentes a mano.
    const [y, m, d] = fecha.split('-').map(Number);
    anio = y;
    mes = m - 1;
    dia = d;
  } else {
    anio = fecha.getFullYear();
    mes = fecha.getMonth();
    dia = fecha.getDate();
  }

  return new Date(anio, mes, dia, horas, minutos, 0, 0);
}

// Toolbar propia de react-big-calendar con el look de Stitch (título + flechas + switch Día/Semana/Mes)
function CalendarToolbar({ label, onNavigate, onView, view }: ToolbarProps<TurnoCalendarEvent>) {
  return (
    <div className="flex flex-wrap justify-between items-center gap-4 p-4 border-b border-surface-container-high">
      <div className="flex items-center gap-3">
        <h1 className="font-headline-lg text-headline-lg text-on-surface capitalize">{label}</h1>
        <div className="flex gap-1">
          <button
            type="button"
            onClick={() => onNavigate('PREV')}
            className="p-2 rounded-lg hover:bg-surface-bright text-secondary transition-colors"
            aria-label="Anterior"
          >
            <span className="material-symbols-outlined">chevron_left</span>
          </button>
          <button
            type="button"
            onClick={() => onNavigate('TODAY')}
            className="px-3 py-2 rounded-lg hover:bg-surface-bright text-secondary font-body-sm text-body-sm transition-colors"
          >
            Hoy
          </button>
          <button
            type="button"
            onClick={() => onNavigate('NEXT')}
            className="p-2 rounded-lg hover:bg-surface-bright text-secondary transition-colors"
            aria-label="Siguiente"
          >
            <span className="material-symbols-outlined">chevron_right</span>
          </button>
        </div>
      </div>

      <div className="flex bg-surface-bright rounded-lg p-1 border border-outline-variant">
        {VISTAS.map((v) => (
          <button
            key={v.value}
            type="button"
            onClick={() => onView(v.value)}
            className={`px-4 py-1.5 rounded-md font-body-sm text-body-sm transition-colors ${
              view === v.value
                ? 'bg-surface-container-lowest shadow-sm font-title-md text-title-md text-primary'
                : 'text-secondary hover:text-primary'
            }`}
          >
            {v.label}
          </button>
        ))}
      </div>
    </div>
  );
}

export function TurnosPage() {
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [servicios, setServicios] = useState<Servicio[]>([]);

  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [drawerOpened, { open: openDrawer, close: closeDrawer }] = useDisclosure(false);
  const [detalleOpened, { open: openDetalle, close: closeDetalle }] = useDisclosure(false);
  const [turnoSeleccionado, setTurnoSeleccionado] = useState<Turno | null>(null);

  const [submitting, setSubmitting] = useState(false);
  const [cancelando, setCancelando] = useState(false);
  const [cambiandoEstado, setCambiandoEstado] = useState(false);

  const [fechaCalendario, setFechaCalendario] = useState(new Date());
  const [vistaCalendario, setVistaCalendario] = useState<'month' | 'week' | 'day'>('week');

  // Filtros del sidebar
  const [recursoFiltro, setRecursoFiltro] = useState<string | null>(null);
  const [serviciosFiltro, setServiciosFiltro] = useState<string[]>([]);

  const form = useForm<TurnoFormValues>({
    initialValues: {
      esClienteNuevo: false,
      clienteId: null,
      clienteNombre: '',
      clienteApellido: '',
      clienteTelefono: '',
      recursoId: null,
      servicioIds: [],
      fecha: null,
      hora: '09:00',
    },
    validate: {
      clienteId: (value, values) =>
        !values.esClienteNuevo && !value ? 'Seleccioná un cliente' : null,
      clienteNombre: (value, values) =>
        values.esClienteNuevo && !value.trim() ? 'El nombre es obligatorio' : null,
      clienteApellido: (value, values) =>
        values.esClienteNuevo && !value.trim() ? 'El apellido es obligatorio' : null,
      recursoId: isNotEmpty('Seleccioná un recurso'),
      servicioIds: (value) => (value.length === 0 ? 'Seleccioná al menos un servicio' : null),
      fecha: (value) => (value === null ? 'Seleccioná una fecha' : null),
      hora: (value) => (!value ? 'Seleccioná una hora' : null),
    },
  });

  useEffect(() => {
    let activo = true;
    const cargar = async () => {
      try {
        const [turnosData, clientesData, recursosData, serviciosData] = await Promise.all([
          turnosService.getAll(),
          clientesService.getAll(),
          recursosService.getAll(),
          serviciosService.getAll(),
        ]);

        if (activo) {
          setTurnos(turnosData);
          setClientes(clientesData);
          setRecursos(recursosData);
          setServicios(serviciosData);
          setErrorMessage(null);
        }
      } catch {
        if (activo) setErrorMessage('No pudimos cargar la información.');
      } finally {
        if (activo) setLoading(false);
      }
    };
    cargar();
    return () => {
      activo = false;
    };
  }, []);

  const recargarTurnos = async () => {
    const data = await turnosService.getAll();
    setTurnos(data);
  };

  const ahora = useMemo(() => new Date(), []);

  const coloresPorRecurso = useMemo(() => {
    const mapa = new Map<string, string>();
    recursos.forEach((r) => mapa.set(r.id.toString(), r.colorHex));
    return mapa;
  }, [recursos]);

  const nombresServiciosFiltrados = useMemo(
    () => new Set(servicios.filter((s) => serviciosFiltro.includes(s.id.toString())).map((s) => s.nombre)),
    [servicios, serviciosFiltro]
  );

  const eventosCalendario = useMemo<TurnoCalendarEvent[]>(() => {
    return turnos
      .filter((t) => t.estado !== 'Cancelado')
      .filter((t) => !recursoFiltro || t.recursoId.toString() === recursoFiltro)
      .filter((t) => nombresServiciosFiltrados.size === 0 || t.servicios.some((s) => nombresServiciosFiltrados.has(s)))
      .map((turno) => {
        const colorDelRecurso = coloresPorRecurso.get(turno.recursoId.toString()) ?? '#0EA5E9';
        return {
          title: recursoFiltro ? turno.clienteNombreCompleto : `${turno.clienteNombreCompleto} — ${turno.recursoNombre}`,
          start: new Date(turno.fechaHoraInicio),
          end: new Date(turno.fechaHoraFin),
          resource: turno,
          colorRecurso: colorDelRecurso,
          recursoId: turno.recursoId,
        };
      });
  }, [turnos, coloresPorRecurso, recursoFiltro, nombresServiciosFiltrados]);

  const { minTime, maxTime } = useMemo(() => {
    const min = new Date();
    min.setHours(8, 0, 0);
    const max = new Date();
    max.setHours(22, 0, 0);
    return { minTime: min, maxTime: max };
  }, []);

  const abrirDrawerNuevoTurno = (fechaInicial?: Date) => {
    form.reset();
    if (fechaInicial) form.setFieldValue('fecha', fechaInicial);
    if (recursoFiltro) form.setFieldValue('recursoId', recursoFiltro);
    openDrawer();
  };

  const handleSelectSlot = (slotInfo: SlotInfo) => {
    form.reset();
    const horaFormateada = format(slotInfo.start, 'HH:mm');
    form.setFieldValue('fecha', slotInfo.start);
    form.setFieldValue('hora', horaFormateada);
    if (recursoFiltro) form.setFieldValue('recursoId', recursoFiltro);
    openDrawer();
  };

  const eventPropGetter = (event: TurnoCalendarEvent) => {
    const esCancelado = event.resource.estado === 'Cancelado';
    const vencido = esTurnoVencido(event.resource, ahora);
    const colorRecurso = coloresPorRecurso.get(event.resource.recursoId.toString()) ?? '#006876';
    const atenuado = esCancelado || vencido;

    return {
      style: {
        backgroundColor: atenuado ? 'var(--mantine-color-gray-2)' : `${colorRecurso}1a`,
        borderLeft: `4px solid ${atenuado ? 'var(--mantine-color-gray-5)' : colorRecurso}`,
        borderTop: 'none',
        borderRight: 'none',
        borderBottom: vencido && !esCancelado ? '2px dashed var(--mantine-color-gray-5)' : 'none',
        opacity: esCancelado ? 0.7 : vencido ? 0.85 : 1,
        color: atenuado ? 'var(--mantine-color-gray-7)' : getContrastTextColor('#ffffff'),
        borderRadius: '8px',
        boxShadow: '0 1px 2px rgba(0,0,0,0.06)',
      },
    };
  };

  const handleSelectEvent = (event: TurnoCalendarEvent) => {
    setTurnoSeleccionado(event.resource);
    openDetalle();
  };

  const handleSubmit = async (values: TurnoFormValues) => {
    if (!values.fecha || !values.recursoId) return;

    setSubmitting(true);
    setErrorMessage(null);

    try {
      const fechaHoraCompleta = combinarFechaYHora(values.fecha, values.hora);

      await turnosService.crear({
        clienteId: values.esClienteNuevo ? null : Number(values.clienteId),
        clienteNuevo: values.esClienteNuevo
          ? {
              nombre: values.clienteNombre,
              apellido: values.clienteApellido,
              telefono: values.clienteTelefono || undefined,
            }
          : null,
        recursoId: Number(values.recursoId),
        servicioIds: values.servicioIds.map(Number),
        fechaHoraInicio: fechaHoraCompleta.toISOString(),
      });

      closeDrawer();
      form.reset();
      await recargarTurnos();

      if (values.esClienteNuevo) {
        const clientesData = await clientesService.getAll();
        setClientes(clientesData);
      }
    } catch (error) {
      if (axios.isAxiosError(error)) {
        const status = error.response?.status;
        const detail = error.response?.data?.detail;

        if (status === 409) {
          setErrorMessage(detail ?? 'El horario seleccionado no está disponible.');
        } else {
          setErrorMessage(detail ?? 'No pudimos crear el turno. Intentá de nuevo.');
        }
      } else {
        setErrorMessage('Ocurrió un error inesperado.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancelar = async (id: number) => {
    setCancelando(true);
    try {
      await turnosService.cancelar(id);
      await recargarTurnos();
      closeDetalle();
      setTurnoSeleccionado(null);
    } catch {
      setErrorMessage('No pudimos cancelar el turno.');
    } finally {
      setCancelando(false);
    }
  };

  const handleCambiarEstado = async (id: number, nuevoEstado: EstadoTurnoEditable) => {
    setCambiandoEstado(true);
    try {
      const turnoActualizado = await turnosService.cambiarEstado(id, { nuevoEstado });
      setTurnoSeleccionado(turnoActualizado);
      setTurnos((prev) => prev.map((t) => (t.id === id ? turnoActualizado : t)));
    } catch {
      setErrorMessage('No pudimos cambiar el estado del turno.');
    } finally {
      setCambiandoEstado(false);
    }
  };

  // --- Mini calendario del sidebar (real: navega el calendario principal) ---
  const diasMiniCalendario = useMemo(() => {
    const inicioMes = startOfMonth(fechaCalendario);
    const finMes = endOfMonth(fechaCalendario);
    const inicioGrilla = startOfWeek(inicioMes, { weekStartsOn: 1 });
    const finGrilla = endOfWeek(finMes, { weekStartsOn: 1 });

    const dias: Date[] = [];
    let cursor = inicioGrilla;
    while (cursor <= finGrilla) {
      dias.push(cursor);
      cursor = addDays(cursor, 1);
    }
    return dias;
  }, [fechaCalendario]);

  const toggleServicioFiltro = (id: string) => {
    setServiciosFiltro((prev) => (prev.includes(id) ? prev.filter((s) => s !== id) : [...prev, id]));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <span className="material-symbols-outlined animate-spin text-primary text-4xl">progress_activity</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4 pb-12">
      <div className="flex items-center justify-between gap-4">
        <h1 className="font-headline-lg text-headline-lg text-on-surface">Agenda de turnos</h1>
        <button
          onClick={() => abrirDrawerNuevoTurno()}
          className="lg:hidden flex items-center gap-2 bg-primary text-on-primary px-5 py-2.5 rounded-full font-title-md text-title-md hover:bg-primary-container transition-colors shadow-sm"
        >
          <span className="material-symbols-outlined text-[20px]">add</span>
          Nuevo turno
        </button>
      </div>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      <div className="flex flex-col lg:flex-row gap-gutter lg:h-[calc(100vh-220px)] lg:min-h-[600px]">
        {/* Sidebar de filtros */}
        <aside className="hidden lg:flex w-64 shrink-0 flex-col gap-6 bg-surface-container-lowest rounded-xl soft-elevation p-6 h-full overflow-y-auto">
          <div>
            <h2 className="font-title-md text-title-md text-on-surface mb-4 flex items-center gap-2">
              <span className="material-symbols-outlined">filter_list</span> Filtros
            </h2>

            <div className="space-y-6">
              {/* Mini calendario */}
              <div className="bg-surface-bright rounded-lg p-3 border border-outline-variant">
                <div className="flex justify-between items-center mb-2">
                  <button
                    type="button"
                    onClick={() => setFechaCalendario((d) => subMonths(d, 1))}
                    className="text-secondary hover:text-primary"
                    aria-label="Mes anterior"
                  >
                    <span className="material-symbols-outlined text-sm">chevron_left</span>
                  </button>
                  <span className="font-label-md text-label-md capitalize">
                    {format(fechaCalendario, 'MMMM yyyy', { locale: es })}
                  </span>
                  <button
                    type="button"
                    onClick={() => setFechaCalendario((d) => addMonths(d, 1))}
                    className="text-secondary hover:text-primary"
                    aria-label="Mes siguiente"
                  >
                    <span className="material-symbols-outlined text-sm">chevron_right</span>
                  </button>
                </div>
                <div className="grid grid-cols-7 gap-1 text-center font-label-md text-label-md text-secondary mb-1">
                  <div>L</div>
                  <div>M</div>
                  <div>M</div>
                  <div>J</div>
                  <div>V</div>
                  <div>S</div>
                  <div>D</div>
                </div>
                <div className="grid grid-cols-7 gap-1 text-center font-body-sm text-body-sm">
                  {diasMiniCalendario.map((dia) => {
                    const esDelMes = isSameMonth(dia, fechaCalendario);
                    const esSeleccionado = isSameDay(dia, fechaCalendario);
                    return (
                      <button
                        type="button"
                        key={dia.toISOString()}
                        onClick={() => setFechaCalendario(dia)}
                        className={`w-6 h-6 rounded-full mx-auto flex items-center justify-center transition-colors ${
                          esSeleccionado
                            ? 'bg-primary text-on-primary'
                            : esDelMes
                              ? 'text-on-surface hover:bg-surface-container'
                              : 'text-outline hover:bg-surface-container'
                        }`}
                      >
                        {dia.getDate()}
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Servicios */}
              {servicios.length > 0 && (
                <div>
                  <h3 className="font-label-md text-label-md text-secondary mb-2 uppercase">Servicios</h3>
                  <div className="space-y-2">
                    {servicios.map((s) => {
                      const activo = serviciosFiltro.includes(s.id.toString());
                      return (
                        <label key={s.id} className="flex items-center gap-2 cursor-pointer group">
                          <span
                            onClick={() => toggleServicioFiltro(s.id.toString())}
                            className={`w-4 h-4 rounded border flex items-center justify-center transition-colors ${
                              activo
                                ? 'bg-primary text-on-primary border-primary'
                                : 'border-outline-variant group-hover:border-primary'
                            }`}
                          >
                            {activo && <span className="material-symbols-outlined text-[12px]">check</span>}
                          </span>
                          <span
                            onClick={() => toggleServicioFiltro(s.id.toString())}
                            className="font-body-sm text-body-sm select-none"
                          >
                            {s.nombre}
                          </span>
                        </label>
                      );
                    })}
                  </div>
                </div>
              )}

              {/* Personal (recursos) */}
              {recursos.length > 0 && (
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="font-label-md text-label-md text-secondary uppercase">Personal</h3>
                    {recursoFiltro && (
                      <button
                        type="button"
                        onClick={() => setRecursoFiltro(null)}
                        className="font-label-md text-label-md text-primary hover:underline"
                      >
                        Ver todos
                      </button>
                    )}
                  </div>
                  <div className="space-y-1">
                    {recursos.map((r) => {
                      const activo = recursoFiltro === r.id.toString();
                      const color = coloresPorRecurso.get(r.id.toString()) ?? '#ccc';
                      const iniciales = r.nombre
                        .split(' ')
                        .map((p) => p[0])
                        .slice(0, 2)
                        .join('')
                        .toUpperCase();
                      return (
                        <button
                          type="button"
                          key={r.id}
                          onClick={() => setRecursoFiltro(activo ? null : r.id.toString())}
                          className={`w-full flex items-center gap-3 p-1.5 rounded-lg transition-colors ${
                            activo ? 'bg-surface-container' : 'hover:bg-surface-bright'
                          }`}
                        >
                          <div
                            className="w-8 h-8 rounded-full flex items-center justify-center font-title-md text-sm shrink-0"
                            style={{ backgroundColor: color, color: getContrastTextColor(color) }}
                          >
                            {iniciales}
                          </div>
                          <span className="font-body-sm text-body-sm text-on-surface truncate">{r.nombre}</span>
                        </button>
                      );
                    })}
                  </div>
                </div>
              )}
            </div>
          </div>

          <button
            onClick={() => abrirDrawerNuevoTurno()}
            className="mt-auto bg-primary hover:bg-on-primary-fixed-variant text-on-primary font-title-md text-title-md py-3 rounded-full flex justify-center items-center gap-2 transition-colors"
          >
            <span className="material-symbols-outlined">add</span> Añadir turno
          </button>
        </aside>

        {/* Calendario */}
        <section className="grow flex flex-col bg-surface-container-lowest rounded-xl soft-elevation overflow-hidden min-h-[600px]">
          <Calendar
            localizer={localizer}
            events={eventosCalendario}
            startAccessor="start"
            endAccessor="end"
            style={{ height: '100%', flex: 1 }}
            selectable
            onSelectSlot={handleSelectSlot}
            onSelectEvent={handleSelectEvent}
            eventPropGetter={eventPropGetter}
            date={fechaCalendario}
            onNavigate={(newDate) => setFechaCalendario(newDate)}
            view={vistaCalendario}
            onView={(newView: string) => setVistaCalendario(newView as 'month' | 'week' | 'day')}
            views={['month', 'week', 'day']}
            culture="es"
            components={{ toolbar: CalendarToolbar }}
            min={minTime}
            max={maxTime}
            messages={{
              next: 'Siguiente',
              previous: 'Anterior',
              today: 'Hoy',
              month: 'Mes',
              week: 'Semana',
              day: 'Día',
              noEventsInRange: 'No hay turnos en este rango.',
            }}
          />
        </section>
      </div>

      {/* DRAWER DE CREACIÓN */}
      <Drawer
        opened={drawerOpened}
        onClose={closeDrawer}
        title={<Text fw={600} size="lg">Programar Turno</Text>}
        position="right"
        size="md"
        padding="lg"
      >
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <Group grow align="flex-start">
              <DatePickerInput
                label="Fecha"
                placeholder="Elegí el día"
                required
                valueFormat="DD/MM/YYYY"
                leftSection={<IconCalendarEvent size={16} style={{ opacity: 0.5 }} />}
                {...form.getInputProps('fecha')}
              />
              <TimeInput
                label="Hora"
                required
                leftSection={<IconClock size={16} style={{ opacity: 0.5 }} />}
                {...form.getInputProps('hora')}
              />
            </Group>

            <Divider my="xs" />

            <Select
              label="Recurso (Lugar/Profesional)"
              placeholder="Elegí el recurso"
              required
              data={recursos.map((r) => ({ value: r.id.toString(), label: r.nombre }))}
              renderOption={({ option, checked }) => (
                <Group flex="1" gap="xs" wrap="nowrap">
                  <ColorSwatch color={coloresPorRecurso.get(option.value) ?? '#ccc'} size={16} />
                  <Text size="sm">{option.label}</Text>
                  {checked && <IconCheck size={16} style={{ marginLeft: 'auto' }} />}
                </Group>
              )}
              leftSection={
                form.values.recursoId ? (
                  <ColorSwatch color={coloresPorRecurso.get(form.values.recursoId) ?? '#ccc'} size={14} />
                ) : (
                  <IconMapPin size={16} style={{ opacity: 0.5 }} />
                )
              }
              {...form.getInputProps('recursoId')}
            />

            <MultiSelect
              label="Servicios a realizar"
              placeholder="Elegí uno o más servicios"
              required
              data={servicios.map((s) => ({ value: s.id.toString(), label: s.nombre }))}
              {...form.getInputProps('servicioIds')}
            />

            <Divider my="xs" />

            <Switch label="Es un cliente nuevo" {...form.getInputProps('esClienteNuevo', { type: 'checkbox' })} />

            {form.values.esClienteNuevo ? (
              <Stack gap="xs" bg="gray.0" p="sm" style={{ borderRadius: 8 }}>
                <Group grow>
                  <TextInput label="Nombre" placeholder="Ej: Laura" required {...form.getInputProps('clienteNombre')} />
                  <TextInput label="Apellido" placeholder="Ej: Gómez" required {...form.getInputProps('clienteApellido')} />
                </Group>
                <TextInput label="Teléfono (Opcional)" placeholder="Ej: 1122334455" {...form.getInputProps('clienteTelefono')} />
              </Stack>
            ) : (
              <Select
                label="Cliente registrado"
                placeholder="Buscá un cliente"
                searchable
                data={clientes.map((c) => ({ value: c.id.toString(), label: `${c.nombre} ${c.apellido}` }))}
                {...form.getInputProps('clienteId')}
              />
            )}

            <Button type="submit" loading={submitting} fullWidth mt="xl" size="md">
              Guardar Turno
            </Button>
          </Stack>
        </form>
      </Drawer>

      {/* MODAL DE DETALLE */}
      <Modal opened={detalleOpened} onClose={closeDetalle} title="Detalle del turno" centered size="sm">
        {turnoSeleccionado && (
          <Stack gap="sm">
            <Group gap="xs">
              <IconUser size={18} color="var(--mantine-color-cyan-6)" />
              <Text fw={500}>{turnoSeleccionado.clienteNombreCompleto}</Text>
            </Group>

            <Group gap="xs">
              <IconMapPin size={18} color="var(--mantine-color-cyan-6)" />
              <Text>{turnoSeleccionado.recursoNombre}</Text>
            </Group>

            <Group gap="xs">
              <IconBriefcase size={18} color="var(--mantine-color-cyan-6)" />
              <Text>{turnoSeleccionado.servicios.join(', ')}</Text>
            </Group>

            <Text size="sm" c="dimmed">
              {new Date(turnoSeleccionado.fechaHoraInicio).toLocaleString('es-AR')} —{' '}
              {new Date(turnoSeleccionado.fechaHoraFin).toLocaleTimeString('es-AR', {
                hour: '2-digit',
                minute: '2-digit',
              })}
            </Text>

            <Group gap="xs">
              <Badge color={COLOR_POR_ESTADO[turnoSeleccionado.estado] ?? 'gray'} variant="light">
                {turnoSeleccionado.estado}
              </Badge>
              <Text size="sm" c="dimmed">
                ${turnoSeleccionado.precioTotal}
              </Text>
            </Group>

            {esTurnoVencido(turnoSeleccionado, ahora) && (
              <Alert icon={<IconAlertCircle size={16} />} color="gray" variant="light">
                La hora de este turno ya pasó y nadie lo marcó como completado o ausente. Actualizá el
                estado abajo.
              </Alert>
            )}

            {turnoSeleccionado.estado !== 'Cancelado' && (
              <Select
                label="Estado del turno"
                data={ESTADOS_EDITABLES}
                value={turnoSeleccionado.estado}
                disabled={cambiandoEstado}
                onChange={(valor) => {
                  if (valor) handleCambiarEstado(turnoSeleccionado.id, valor as EstadoTurnoEditable);
                }}
              />
            )}

            {turnoSeleccionado.estado !== 'Cancelado' && (
              <Button
                color="red"
                variant="light"
                fullWidth
                mt="sm"
                loading={cancelando}
                onClick={() => handleCancelar(turnoSeleccionado.id)}
              >
                Cancelar turno
              </Button>
            )}
          </Stack>
        )}
      </Modal>

      <style>{`
        .rbc-toolbar { display: none; }
        .rbc-time-view, .rbc-month-view {
          border: none;
        }
        .rbc-time-header-content, .rbc-time-content, .rbc-month-row {
          border-color: #dce9ff;
        }
        .rbc-header {
          background: #eff4ff;
          text-transform: uppercase;
          font-weight: 600;
          font-size: 12px;
          color: #4d6265;
          padding: 10px 4px;
          border-color: #dce9ff;
        }
        .rbc-today {
          background-color: rgba(0, 188, 212, 0.06);
        }
        .rbc-off-range-bg {
          background: #f8f9ff;
        }
        .rbc-event {
          padding: 2px 8px;
        }
        .rbc-event:focus {
          outline: 2px solid #006876;
        }
        .rbc-current-time-indicator {
          background-color: #ba1a1a;
          height: 2px;
        }
        .rbc-time-slot {
          border-color: #eff4ff;
        }
        .rbc-timeslot-group {
          border-color: #dce9ff;
        }
      `}</style>
    </div>
  );
}
