// src/pages/Servicios/ServiciosPage.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Button,
  Group,
  Table,
  Modal,
  TextInput,
  Textarea,
  NumberInput,
  Stack,
  Alert,
  Loader,
  Center,
  Text,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconPlus, IconAlertCircle } from '@tabler/icons-react';
import { serviciosService } from '../../api/servicioService';
import type { Servicio } from '../../types/Servicio';

export function ServiciosPage() {
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [submitting, setSubmitting] = useState(false);

  const form = useForm({
    initialValues: { nombre: '', descripcion: '', duracionMinutos: 30, precio: 0 },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      duracionMinutos: (value) => (value > 0 ? null : 'La duración debe ser mayor a 0'),
      precio: (value) => (value >= 0 ? null : 'El precio no puede ser negativo'),
    },
  });

  // Carga inicial — con flag de cancelación para evitar setState en un componente
  // ya desmontado (relevante en React Strict Mode y si el usuario navega rápido).
  useEffect(() => {
    let activo = true;

    const cargar = async () => {
      try {
        const data = await serviciosService.getAll();
        if (activo) {
          setServicios(data);
          setErrorMessage(null);
        }
      } catch {
        if (activo) {
          setErrorMessage('No pudimos cargar los servicios. Intentá de nuevo.');
        }
      } finally {
        if (activo) {
          setLoading(false);
        }
      }
    };

    cargar();

    return () => {
      activo = false;
    };
  }, []);

  // Recarga manual — se usa después de crear un servicio nuevo.
  const recargarServicios = async () => {
    setLoading(true);
    try {
      const data = await serviciosService.getAll();
      setServicios(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los servicios. Intentá de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      await serviciosService.create(values);
      closeModal();
      form.reset();
      await recargarServicios();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        form.setFieldError('nombre', error.response.data.detail);
      } else {
        setErrorMessage('No pudimos crear el servicio. Intentá de nuevo.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Gestión de Servicios</Title>
        <Button leftSection={<IconPlus size={16} />} color="cyan" onClick={openModal}>
          Nuevo servicio
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
      ) : servicios.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          Todavía no cargaste ningún servicio.
        </Text>
      ) : (
        <Table striped highlightOnHover verticalSpacing="sm">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Nombre</Table.Th>
              <Table.Th>Descripción</Table.Th>
              <Table.Th>Duración</Table.Th>
              <Table.Th>Precio</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {servicios.map((servicio) => (
              <Table.Tr key={servicio.id}>
                <Table.Td>{servicio.nombre}</Table.Td>
                <Table.Td>{servicio.descripcion}</Table.Td>
                <Table.Td>{servicio.duracionMinutos} min</Table.Td>
                <Table.Td>${servicio.precio}</Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      <Modal opened={modalOpened} onClose={closeModal} title="Nuevo servicio" centered>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <TextInput
              label="Nombre"
              placeholder="Ej: Consulta general"
              required
              {...form.getInputProps('nombre')}
            />
            <Textarea
              label="Descripción"
              placeholder="Detalle opcional"
              {...form.getInputProps('descripcion')}
            />
            <NumberInput
              label="Duración (minutos)"
              min={1}
              required
              {...form.getInputProps('duracionMinutos')}
            />
            <NumberInput
              label="Precio"
              min={0}
              decimalScale={2}
              prefix="$"
              required
              {...form.getInputProps('precio')}
            />
            <Button type="submit" color="cyan" loading={submitting} fullWidth mt="sm">
              Guardar
            </Button>
          </Stack>
        </form>
      </Modal>
    </Stack>
  );
}