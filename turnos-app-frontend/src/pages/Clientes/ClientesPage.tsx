// src/pages/Clientes/ClientesPage.tsx
import { useEffect, useState, useMemo } from 'react';
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
import { IconPlus, IconAlertCircle, IconSearch } from '@tabler/icons-react';
import { clientesService } from '../../api/clientesService';
import type { Cliente } from '../../types/Cliente';

export function ClientesPage() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  
  const [searchTerm, setSearchTerm] = useState('');
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [submitting, setSubmitting] = useState(false);
  const [clienteEditandoId, setClienteEditandoId] = useState<number | null>(null);

  const form = useForm({
    initialValues: { nombre: '', apellido: '', email: '', telefono: '' },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      apellido: isNotEmpty('El apellido es obligatorio'),
    },
  });

  // Lógica de búsqueda reactiva
  const clientesFiltrados = useMemo(() => {
    return clientes.filter((c) => 
      c.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
      c.apellido.toLowerCase().includes(searchTerm.toLowerCase()) ||
      c.email?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [clientes, searchTerm]);

  useEffect(() => {
    const cargar = async () => {
      setLoading(true);
      try {
        const data = await clientesService.getAll();
        setClientes(data);
      } catch {
        setErrorMessage('No pudimos cargar los clientes.');
      } finally {
        setLoading(false);
      }
    };
    cargar();
  }, []);

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      if (clienteEditandoId) {
        await clientesService.update(clienteEditandoId, values);
      } else {
        await clientesService.create(values);
      }
      closeModal();
      form.reset();
      setClienteEditandoId(null);
      const data = await clientesService.getAll();
      setClientes(data);
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        form.setFieldError('nombre', error.response.data.detail);
      } else {
        setErrorMessage('No pudimos guardar los cambios.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Gestión de Clientes</Title>
        <Button leftSection={<IconPlus size={16} />} color="cyan" onClick={() => {
          form.reset();
          setClienteEditandoId(null);
          openModal();
        }}>
          Nuevo cliente
        </Button>
      </Group>

      <TextInput
        placeholder="Buscar por nombre, apellido o email..."
        leftSection={<IconSearch size={16} />}
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.currentTarget.value)}
        maw={400}
      />

      {errorMessage && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" variant="light">
          {errorMessage}
        </Alert>
      )}

      {loading ? (
        <Center py="xl"> <Loader color="cyan" /> </Center>
      ) : clientesFiltrados.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">No se encontraron clientes.</Text>
      ) : (
        <Table striped highlightOnHover verticalSpacing="sm">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Nombre</Table.Th>
              <Table.Th>Apellido</Table.Th>
              <Table.Th>Teléfono</Table.Th>
              <Table.Th>Email</Table.Th>
              <Table.Th>Acciones</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {clientesFiltrados.map((c) => (
              <Table.Tr key={c.id}>
                <Table.Td>{c.nombre}</Table.Td>
                <Table.Td>{c.apellido}</Table.Td>
                <Table.Td>{c.telefono ?? '—'}</Table.Td>
                <Table.Td>{c.email ?? '—'}</Table.Td>
                <Table.Td>
                  <Button 
                    variant="light" 
                    color="cyan" 
                    size="xs" 
                    onClick={() => {
                      // Saneamos los datos aquí mismo:
                      // Convertimos null a undefined o string vacío según lo que tu formulario necesite
                      form.setValues({
                        nombre: c.nombre,
                        apellido: c.apellido,
                        email: c.email ?? '',      // Si es null, devuelve string vacío
                        telefono: c.telefono ?? '' // Si es null, devuelve string vacío
                      });
                      
                      setClienteEditandoId(c.id);
                      openModal();
                    }}
                  >
                    Editar
                  </Button>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      <Modal opened={modalOpened} onClose={() => { closeModal(); setClienteEditandoId(null); }} 
             title={clienteEditandoId ? "Editar cliente" : "Nuevo cliente"} centered>
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