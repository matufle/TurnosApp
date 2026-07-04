// src/pages/Recursos/RecursosPage.tsx
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
  Stack,
  Alert,
  Loader,
  Center,
  Text,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconPlus, IconAlertCircle } from '@tabler/icons-react';
import { recursosService } from '../../api/recursosService';
import type { Recurso } from '../../types/Recurso';

export function RecursosPage() {
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [submitting, setSubmitting] = useState(false);

  const form = useForm({
    initialValues: { nombre: '', descripcion: '' },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
    },
  });

  // Carga inicial — con flag de cancelación para evitar setState en un componente
  // ya desmontado (relevante en React Strict Mode y si el usuario navega rápido).
  useEffect(() => {
    let activo = true;

    const cargar = async () => {
      try {
        const data = await recursosService.getAll();
        if (activo) {
          setRecursos(data);
          setErrorMessage(null);
        }
      } catch {
        if (activo) {
          setErrorMessage('No pudimos cargar los recursos. Intentá de nuevo.');
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

  // Recarga manual — se usa después de crear un recurso nuevo.
  // Al ser disparada desde un event handler (submit), no tiene el mismo
  // riesgo de carrera que el efecto de carga inicial.
  const recargarRecursos = async () => {
    setLoading(true);
    try {
      const data = await recursosService.getAll();
      setRecursos(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los recursos. Intentá de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      await recursosService.create(values);
      closeModal();
      form.reset();
      await recargarRecursos(); // recarga la tabla con el nuevo recurso
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        form.setFieldError('nombre', error.response.data.detail);
      } else {
        setErrorMessage('No pudimos crear el recurso. Intentá de nuevo.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Gestión de Recursos</Title>
        <Button leftSection={<IconPlus size={16} />} color="cyan" onClick={openModal}>
          Nuevo recurso
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
      ) : recursos.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          Todavía no cargaste ningún recurso.
        </Text>
      ) : (
        <Table striped highlightOnHover verticalSpacing="sm">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Nombre</Table.Th>
              <Table.Th>Descripción</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {recursos.map((recurso) => (
              <Table.Tr key={recurso.id}>
                <Table.Td>{recurso.nombre}</Table.Td>
                <Table.Td>{recurso.descripcion}</Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      <Modal opened={modalOpened} onClose={closeModal} title="Nuevo recurso" centered>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <TextInput
              label="Nombre"
              placeholder="Ej: Consultorio 1"
              required
              {...form.getInputProps('nombre')}
            />
            <Textarea
              label="Descripción"
              placeholder="Detalle opcional"
              {...form.getInputProps('descripcion')}
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