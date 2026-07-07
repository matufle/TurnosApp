// src/pages/Turnos/TurnosPage.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Button,
  Group,
  Table,
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
} from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconPlus, IconAlertCircle } from '@tabler/icons-react';
import { turnosService } from '../../api/turnosService';
import { clientesService } from '../../api/clientesService';
import { recursosService } from '../../api/recursosService';
import { serviciosService } from '../../api/servicioService';
import type { Turno } from '../../types/Turno';
import type { Cliente } from '../../types/Cliente';
import type { Recurso } from '../../types/Recurso';
import type { Servicio } from '../../types/Servicio';

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

export function TurnosPage() {
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [servicios, setServicios] = useState<Servicio[]>([]);

  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [submitting, setSubmitting] = useState(false);

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
        // Fix: DateTimePicker de Mantine puede devolver string en vez de Date.
        // new Date(...) normaliza ambos casos antes de llamar a toISOString().
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
      // Log completo del error real — nunca más "se traga" el detalle.
      console.error('Error real al crear turno:', error);

      if (axios.isAxiosError(error)) {
        console.error('Response data:', error.response?.data);

        const status = error.response?.status;
        const detail = error.response?.data?.detail;

        if (status === 409) {
          // Conflicto: solapamiento de horario (SolapamientoException / BusinessException)
          setErrorMessage(detail ?? 'El horario seleccionado no está disponible para ese recurso.');
        } else if (status === 400) {
          // Bad Request: validación de negocio (ej: cliente/servicio requerido)
          setErrorMessage(detail ?? 'Revisá los datos ingresados, hay un problema con la solicitud.');
        } else if (status === 404) {
          // Not Found: cliente, recurso o servicio inexistente
          setErrorMessage(detail ?? 'Alguno de los datos seleccionados ya no existe.');
        } else {
          setErrorMessage(detail ?? 'No pudimos crear el turno. Intentá de nuevo.');
        }
      } else {
        // Error que no vino de axios (ej: error de JS antes de la request, como el bug de fecha original)
        setErrorMessage('Ocurrió un error inesperado al procesar el formulario.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancelar = async (id: number) => {
    try {
      await turnosService.cancelar(id);
      await recargarTurnos();
    } catch (error) {
      console.error('Error al cancelar turno:', error);
      setErrorMessage('No pudimos cancelar el turno.');
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Agenda de Turnos</Title>
        <Button leftSection={<IconPlus size={16} />} color="cyan" onClick={openModal}>
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
      ) : turnos.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          Todavía no hay turnos cargados.
        </Text>
      ) : (
        <Table striped highlightOnHover verticalSpacing="sm">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Cliente</Table.Th>
              <Table.Th>Recurso</Table.Th>
              <Table.Th>Servicios</Table.Th>
              <Table.Th>Inicio</Table.Th>
              <Table.Th>Estado</Table.Th>
              <Table.Th>Precio</Table.Th>
              <Table.Th />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {turnos.map((turno) => (
              <Table.Tr key={turno.id}>
                <Table.Td>{turno.clienteNombreCompleto}</Table.Td>
                <Table.Td>{turno.recursoNombre}</Table.Td>
                <Table.Td>{turno.servicios.join(', ')}</Table.Td>
                <Table.Td>{new Date(turno.fechaHoraInicio).toLocaleString('es-AR')}</Table.Td>
                <Table.Td>
                  <Badge color={turno.estado === 'Cancelado' ? 'red' : 'cyan'} variant="light">
                    {turno.estado}
                  </Badge>
                </Table.Td>
                <Table.Td>${turno.precioTotal}</Table.Td>
                <Table.Td>
                  {turno.estado !== 'Cancelado' && (
                    <Button size="xs" color="red" variant="subtle" onClick={() => handleCancelar(turno.id)}>
                      Cancelar
                    </Button>
                  )}
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

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
                <TextInput
                  label="Nombre"
                  placeholder="Laura"
                  required
                  {...form.getInputProps('clienteNombre')}
                />
                <TextInput
                  label="Apellido"
                  placeholder="Gómez"
                  required
                  {...form.getInputProps('clienteApellido')}
                />
                <TextInput
                  label="Teléfono"
                  placeholder="1122334455"
                  {...form.getInputProps('clienteTelefono')}
                />
              </Stack>
            ) : (
              <Select
                label="Cliente"
                placeholder="Buscá un cliente existente"
                searchable
                data={clientes.map((c) => ({
                  value: c.id.toString(),
                  label: `${c.nombre} ${c.apellido}`,
                }))}
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
    </Stack>
  );
}