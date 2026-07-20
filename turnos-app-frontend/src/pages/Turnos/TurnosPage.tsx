// src/pages/Turnos/TurnosPage.tsx
import { useEffect, useMemo, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Button,
  Group,
  Modal,
  Drawer,
  TextInput,
  Select,
  MultiSelect,
  Stack,
  Alert,
  Loader,
  Center,
  Text,
  Switch,
  Badge,
  Paper,
  ColorSwatch,
  Divider,
} from '@mantine/core';
import { DatePickerInput, TimeInput } from '@mantine/dates';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import {
  IconPlus,
  IconAlertCircle,
  IconUser,
  IconMapPin,
  IconBriefcase,
  IconCheck,
  IconClock,
  IconCalendarEvent,
  IconFilter,
} from '@tabler/icons-react';
import { Calendar, dateFnsLocalizer } from 'react-big-calendar';
import type { SlotInfo, Event as CalendarEvent } from 'react-big-calendar';
import { format } from 'date-fns/format';
import { parse } from 'date-fns/parse';
import { startOfWeek } from 'date-fns/startOfWeek';
import { getDay } from 'date-fns/getDay';
import { es } from 'date-fns/locale/es';

import '@mantine/dates/styles.css';
import 'react-big-calendar/lib/css/react-big-calendar.css';

import { turnosService } from '../../api/turnosService';
import { clientesService } from '../../api/clientesService';
import { recursosService } from '../../api/recursosService';
import { serviciosService } from '../../api/servicioService';
import type { Turno } from '../../types/Turno';
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

  const [fechaCalendario, setFechaCalendario] = useState(new Date());
  const [vistaCalendario, setVistaCalendario] = useState<'month' | 'week' | 'day'>('week');

  // NUEVO ESTADO: Filtro de recurso
  const [recursoFiltro, setRecursoFiltro] = useState<string | null>(null);

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
    return () => { activo = false; };
  }, []);

  const recargarTurnos = async () => {
    const data = await turnosService.getAll();
    setTurnos(data);
  };

  const coloresPorRecurso = useMemo(() => {
    const mapa = new Map<string, string>();
    recursos.forEach((r) => mapa.set(r.id.toString(), r.colorHex));
    return mapa;
  }, [recursos]);

  // EVENTOS FILTRADOS
  const eventosCalendario = useMemo<TurnoCalendarEvent[]>(() => {
    return turnos
      .filter((t) => t.estado !== 'Cancelado')
      // Filtramos si el usuario seleccionó un recurso específico
      .filter((t) => !recursoFiltro || t.recursoId.toString() === recursoFiltro)
      .map((turno) => {
        const colorDelRecurso = coloresPorRecurso.get(turno.recursoId.toString()) ?? '#0EA5E9';
        return {
          // Si hay filtro activo, solo mostramos el nombre del cliente (más limpio)
          // Si vemos "Todos", mostramos Cliente — Recurso
          title: recursoFiltro 
            ? turno.clienteNombreCompleto 
            : `${turno.clienteNombreCompleto} — ${turno.recursoNombre}`,
          start: new Date(turno.fechaHoraInicio),
          end: new Date(turno.fechaHoraFin),
          resource: turno,
          colorRecurso: colorDelRecurso,
          recursoId: turno.recursoId,
        };
      });
  }, [turnos, coloresPorRecurso, recursoFiltro]);

  // Cortamos la madrugada para que las cajas se vean más grandes (De 08:00 a 22:00)
  const { minTime, maxTime } = useMemo(() => {
    const min = new Date();
    min.setHours(8, 0, 0); 
    const max = new Date();
    max.setHours(22, 0, 0); 
    return { minTime: min, maxTime: max };
  }, []);

  const handleSelectSlot = (slotInfo: SlotInfo) => {
    form.reset();
    const horaFormateada = format(slotInfo.start, 'HH:mm');
    form.setFieldValue('fecha', slotInfo.start);
    form.setFieldValue('hora', horaFormateada);
    
    // TRUCO DE UX: Si estamos filtrando un recurso, lo pre-seleccionamos en el formulario
    if (recursoFiltro) {
      form.setFieldValue('recursoId', recursoFiltro);
    }
    
    openDrawer();
  };

  const eventPropGetter = (event: TurnoCalendarEvent) => {
    const esCancelado = event.resource.estado === 'Cancelado';
    const colorRecurso = coloresPorRecurso.get(event.resource.recursoId.toString());
    const backgroundColor = esCancelado ? 'var(--mantine-color-gray-5)' : colorRecurso ?? 'var(--mantine-color-cyan-6)';
    const textColor = esCancelado ? '#ffffff' : colorRecurso ? getContrastTextColor(colorRecurso) : '#ffffff';

    return {
      style: {
        backgroundColor,
        borderColor: esCancelado ? 'var(--mantine-color-gray-6)' : backgroundColor,
        opacity: esCancelado ? 0.6 : 1,
        color: textColor,
        borderRadius: '6px',
        border: 'none',
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
        
        // 👇 ACÁ ESTÁ EL CAMBIO: Usamos toISOString() directo
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

  return (
    <Stack gap="lg">
      <Group justify="space-between" align="flex-end">
        <Stack gap="xs">
          <Title order={2}>Agenda de Turnos</Title>
          
          {/* EL NUEVO FILTRO DE RECURSOS */}
          <Select
            placeholder="Todos los recursos"
            data={recursos.map((r) => ({ value: r.id.toString(), label: r.nombre }))}
            value={recursoFiltro}
            onChange={setRecursoFiltro}
            clearable // Permite volver a ver "Todos" tocando la crucecita
            leftSection={<IconFilter size={16} style={{ opacity: 0.5 }} />}
            style={{ width: 250 }}
            renderOption={({ option, checked }) => (
              <Group flex="1" gap="xs" wrap="nowrap">
                <ColorSwatch color={coloresPorRecurso.get(option.value) ?? '#ccc'} size={12} />
                <Text size="sm">{option.label}</Text>
                {checked && <IconCheck size={16} style={{ marginLeft: 'auto' }} />}
              </Group>
            )}
          />
        </Stack>

        <Button
          leftSection={<IconPlus size={16} />}
          onClick={() => {
            form.reset();
            // Si hay un recurso filtrado, lo mandamos al form
            if (recursoFiltro) {
              form.setFieldValue('recursoId', recursoFiltro);
            }
            openDrawer();
          }}
        >
          Nuevo turno
        </Button>
      </Group>

      {errorMessage && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" variant="light">
          {errorMessage}
        </Alert>
      )}

      {loading ? (
        <Center py="xl">
          <Loader/>
        </Center>
      ) : (
        <Paper withBorder radius="md" p="md">
          <Calendar
            localizer={localizer}
            events={eventosCalendario} // Usa los eventos ya filtrados
            startAccessor="start"
            endAccessor="end"
            style={{ height: 650 }}
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
            
            // Mantenemos el límite horario para que no se vea la madrugada
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
        </Paper>
      )}

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

            <Switch
              label="Es un cliente nuevo"
              {...form.getInputProps('esClienteNuevo', { type: 'checkbox' })}
            />

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
              <Badge color={turnoSeleccionado.estado === 'Cancelado' ? 'red' : 'cyan'} variant="light">
                {turnoSeleccionado.estado}
              </Badge>
              <Text size="sm" c="dimmed">
                ${turnoSeleccionado.precioTotal}
              </Text>
            </Group>

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
        .rbc-toolbar button {
          color: var(--mantine-color-cyan-6);
          border-color: var(--mantine-color-cyan-2);
        }
        .rbc-toolbar button:hover {
          background-color: var(--mantine-color-cyan-0);
          border-color: var(--mantine-color-cyan-6);
        }
        .rbc-toolbar button.rbc-active {
          background-color: var(--mantine-color-cyan-6);
          border-color: var(--mantine-color-cyan-6);
          color: white;
        }
        .rbc-today {
          background-color: var(--mantine-color-cyan-0);
        }
        .rbc-event {
          padding: 2px 6px;
        }
        .rbc-event:focus {
          outline: 2px solid var(--mantine-color-cyan-6);
        }
      `}</style>
    </Stack>
  );
}