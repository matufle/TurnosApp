// src/pages/Clientes/ClientesPage.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Button,
  Group,
  Table,
  Modal,
  TextInput,
  Stack,
  Alert,
  Loader,
  Center,
  Text,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconPlus, IconAlertCircle } from '@tabler/icons-react';
import { clientesService } from '../../api/clientesService';
import type { Cliente } from '../../types/Cliente';

export function ClientesPage() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [submitting, setSubmitting] = useState(false);

  const form = useForm({
    initialValues: { nombre: '', apellido: '', email: '', telefono: '' },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      apellido: isNotEmpty('El apellido es obligatorio'),
    },
  });

  useEffect(() => {
    let activo = true;

    const cargar = async () => {
      try {
        const data = await clientesService.getAll();
        if (activo) {
          setClientes(data);
          setErrorMessage(null);
        }
      } catch {
        if (activo) setErrorMessage('No pudimos cargar los clientes. Intentá de nuevo.');
      } finally {
        if (activo) setLoading(false);
      }
    };

    cargar();
    return () => {
      activo = false;
    };
  }, []);

  const recargarClientes = async () => {
    setLoading(true);
    try {
      const data = await clientesService.getAll();
      setClientes(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los clientes. Intentá de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      await clientesService.create(values);
      closeModal();
      form.reset();
      await recargarClientes();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        form.setFieldError('nombre', error.response.data.detail);
      } else {
        setErrorMessage('No pudimos crear el cliente. Intentá de nuevo.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Gestión de Clientes</Title>
        <Button leftSection={<IconPlus size={16} />} color="cyan" onClick={openModal}>
          Nuevo cliente
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
      ) : clientes.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          Todavía no cargaste ningún cliente.
        </Text>
      ) : (
        <Table striped highlightOnHover verticalSpacing="sm">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Nombre</Table.Th>
              <Table.Th>Apellido</Table.Th>
              <Table.Th>Teléfono</Table.Th>
              <Table.Th>Email</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {clientes.map((cliente) => (
              <Table.Tr key={cliente.id}>
                <Table.Td>{cliente.nombre}</Table.Td>
                <Table.Td>{cliente.apellido}</Table.Td>
                <Table.Td>{cliente.telefono ?? '—'}</Table.Td>
                <Table.Td>{cliente.email ?? '—'}</Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      <Modal opened={modalOpened} onClose={closeModal} title="Nuevo cliente" centered>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <TextInput label="Nombre" required {...form.getInputProps('nombre')} />
            <TextInput label="Apellido" required {...form.getInputProps('apellido')} />
            <TextInput label="Teléfono" {...form.getInputProps('telefono')} />
            <TextInput label="Email" {...form.getInputProps('email')} />
            <Button type="submit" color="cyan" loading={submitting} fullWidth mt="sm">
              Guardar
            </Button>
          </Stack>
        </form>
      </Modal>
    </Stack>
  );
}