// src/pages/Turnos/TurnosPage.tsx
import { useEffect, useMemo, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Button,
  Group,
  Modal,
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
} from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconPlus, IconAlertCircle, IconUser, IconMapPin, IconBriefcase } from '@tabler/icons-react';
import { Calendar, dateFnsLocalizer } from 'react-big-calendar';
import type { SlotInfo } from 'react-big-calendar';
import type { Event as CalendarEvent } from 'react-big-calendar';
import {format} from 'date-fns/format';
import {parse} from 'date-fns/parse';
import {startOfWeek} from 'date-fns/startOfWeek';
import {getDay} from 'date-fns/getDay';
import {es} from 'date-fns/locale/es';
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
  fechaHoraInicio: Date | null;
}

// Evento enriquecido: react-big-calendar necesita start/end/title,
// pero guardamos el Turno completo en `resource` para no perder datos al hacer click.
interface TurnoCalendarEvent extends CalendarEvent {
  resource: Turno;
  colorRecurso: string;
}

export function TurnosPage() {
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [servicios, setServicios] = useState<Servicio[]>([]);
  

  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [detalleOpened, { open: openDetalle, close: closeDetalle }] = useDisclosure(false);
  const [turnoSeleccionado, setTurnoSeleccionado] = useState<Turno | null>(null);

  const [submitting, setSubmitting] = useState(false);
  const [cancelando, setCancelando] = useState(false);

  // Estados para controlar la navegación del calendario
  const [fechaCalendario, setFechaCalendario] = useState(new Date());
  const [vistaCalendario, setVistaCalendario] = useState<'month' | 'week' | 'day'>('week');

  const form = useForm<TurnoFormValues>({
    initialValues: {
      esClienteNuevo: false,
      clienteId: null,
      clienteNombre: '',
      clienteApellido: '',
      clienteTelefono: '',
      recursoId: null,
      servicioIds: [],
      fechaHoraInicio: null,
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
      fechaHoraInicio: (value) => (value === null ? 'Seleccioná fecha y hora' : null),
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
      } catch (error) {
        console.error('Error al cargar datos iniciales:', error);
        if (activo) setErrorMessage('No pudimos cargar la información. Intentá de nuevo.');
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

  // ---- Mapeo Turno[] -> eventos de react-big-calendar ----
const eventosCalendario = useMemo<TurnoCalendarEvent[]>(() => {
  return turnos
    .filter((t) => t.estado !== 'Cancelado')
    .map((turno) => {
      // Buscamos el recurso de este turno en nuestra lista de recursos
      const recurso = recursos.find((r) => r.id === turno.recursoId);
      const colorDelRecurso = recurso?.colorHex || '#0EA5E9'; // Cyan por si falla algo

      return {
        title: `${turno.clienteNombreCompleto} — ${turno.recursoNombre}`,
        start: new Date(turno.fechaHoraInicio),
        end: new Date(turno.fechaHoraFin),
        resource: turno,
        colorRecurso: colorDelRecurso, // Lo guardamos en el evento
      };
    });
}, [turnos, recursos]);


  // ---- Click en casillero vacío: pre-carga la fecha y abre el modal ----
  const handleSelectSlot = (slotInfo: SlotInfo) => {
    form.reset();
    form.setFieldValue('fechaHoraInicio', slotInfo.start);
    openModal();
  };

  // Mapa recursoId -> colorHex, para no hacer un .find() en cada evento del calendario.
const coloresPorRecurso = useMemo(() => {
  const mapa = new Map<number, string>();
  recursos.forEach((r) => mapa.set(r.id, r.colorHex));
  return mapa;
}, [recursos]);

const eventPropGetter = (event: TurnoCalendarEvent) => {
  const esCancelado = event.resource.estado === 'Cancelado';

  // Color del recurso asignado a este turno; fallback a Cyan del theme
  // si por algún motivo el recurso no tiene color cargado.
  const colorRecurso = coloresPorRecurso.get(event.resource.recursoId);
  const backgroundColor = esCancelado
    ? 'var(--mantine-color-gray-5)'
    : colorRecurso ?? 'var(--mantine-color-cyan-6)';

  const textColor = esCancelado
    ? '#ffffff'
    : colorRecurso
      ? getContrastTextColor(colorRecurso)
      : '#ffffff';

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

  // ---- Click en un turno existente: abre el modal de detalle ----
  const handleSelectEvent = (event: TurnoCalendarEvent) => {
    setTurnoSeleccionado(event.resource);
    openDetalle();
  };

  const handleSubmit = async (values: TurnoFormValues) => {
    if (!values.fechaHoraInicio || !values.recursoId) return;

    setSubmitting(true);
    setErrorMessage(null);

    try {
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
        fechaHoraInicio: new Date(values.fechaHoraInicio).toISOString(),
      });

      closeModal();
      form.reset();
      await recargarTurnos();

      if (values.esClienteNuevo) {
        const clientesData = await clientesService.getAll();
        setClientes(clientesData);
      }
    } catch (error) {
      console.error('Error real al crear turno:', error);

      if (axios.isAxiosError(error)) {
        console.error('Response data:', error.response?.data);

        const status = error.response?.status;
        const detail = error.response?.data?.detail;

        if (status === 409) {
          setErrorMessage(detail ?? 'El horario seleccionado no está disponible para ese recurso.');
        } else if (status === 400) {
          setErrorMessage(detail ?? 'Revisá los datos ingresados, hay un problema con la solicitud.');
        } else if (status === 404) {
          setErrorMessage(detail ?? 'Alguno de los datos seleccionados ya no existe.');
        } else {
          setErrorMessage(detail ?? 'No pudimos crear el turno. Intentá de nuevo.');
        }
      } else {
        setErrorMessage('Ocurrió un error inesperado al procesar el formulario.');
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
    } catch (error) {
      console.error('Error al cancelar turno:', error);
      setErrorMessage('No pudimos cancelar el turno.');
    } finally {
      setCancelando(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Agenda de Turnos</Title>
        <Button
          leftSection={<IconPlus size={16} />}
          color="cyan"
          onClick={() => {
            form.reset();
            openModal();
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
          <Loader color="cyan" />
        </Center>
      ) : (
        <Paper withBorder radius="md" p="md">
          <Calendar
            localizer={localizer}
            events={eventosCalendario}
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

      {/* Modal de creación */}
      <Modal opened={modalOpened} onClose={closeModal} title="Nuevo turno" centered size="md">
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <Switch
              label="Cliente nuevo"
              description="Activá esto si el cliente todavía no está registrado"
              color="cyan"
              {...form.getInputProps('esClienteNuevo', { type: 'checkbox' })}
            />

            {form.values.esClienteNuevo ? (
              <Stack gap="sm">
                <TextInput label="Nombre" placeholder="Laura" required {...form.getInputProps('clienteNombre')} />
                <TextInput label="Apellido" placeholder="Gómez" required {...form.getInputProps('clienteApellido')} />
                <TextInput label="Teléfono" placeholder="1122334455" {...form.getInputProps('clienteTelefono')} />
              </Stack>
            ) : (
              <Select
                label="Cliente"
                placeholder="Buscá un cliente existente"
                searchable
                data={clientes.map((c) => ({ value: c.id.toString(), label: `${c.nombre} ${c.apellido}` }))}
                {...form.getInputProps('clienteId')}
              />
            )}

            <Select
              label="Recurso"
              placeholder="Elegí el recurso"
              required
              data={recursos.map((r) => ({ value: r.id.toString(), label: r.nombre }))}
              {...form.getInputProps('recursoId')}
            />

            <MultiSelect
              label="Servicios"
              placeholder="Elegí uno o más servicios"
              required
              data={servicios.map((s) => ({ value: s.id.toString(), label: s.nombre }))}
              {...form.getInputProps('servicioIds')}
            />

            <DateTimePicker
              label="Fecha y hora de inicio"
              placeholder="Elegí cuándo"
              required
              {...form.getInputProps('fechaHoraInicio')}
            />

            <Button type="submit" color="cyan" loading={submitting} fullWidth mt="sm">
              Reservar turno
            </Button>
          </Stack>
        </form>
      </Modal>

      {/* Modal de detalle */}
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