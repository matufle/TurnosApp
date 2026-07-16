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
  ActionIcon, // Importamos ActionIcon para el botón de basura
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconPlus, IconAlertCircle, IconSearch, IconTrash, IconUsers } from '@tabler/icons-react'; // Sumamos IconTrash
import { clientesService } from '../../api/clientesService';
import type { Cliente } from '../../types/Cliente';
import { EmptyState } from '../../components/EmptyState';

export function ClientesPage() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  
  const [searchTerm, setSearchTerm] = useState('');
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [submitting, setSubmitting] = useState(false);
  const [clienteEditandoId, setClienteEditandoId] = useState<number | null>(null);

  // Nuevos estados para manejar la eliminación
  const [clienteAEliminar, setClienteAEliminar] = useState<Cliente | null>(null);
  const [eliminando, setEliminando] = useState(false);

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
      (c.email?.toLowerCase().includes(searchTerm.toLowerCase()))
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

  // Función para manejar el borrado
  const handleEliminar = async () => {
    if (!clienteAEliminar) return;
    setEliminando(true);
    try {
      // Usamos el id del cliente que guardamos en el estado temporal
      await clientesService.delete(clienteAEliminar.id);
      
      // Actualizamos la tabla sacando el cliente borrado sin volver a llamar a la API
      setClientes((prev) => prev.filter((c) => c.id !== clienteAEliminar.id));
      setClienteAEliminar(null);
    } catch{
      setErrorMessage('No pudimos eliminar el cliente. Puede que tenga turnos asociados.');
    } finally {
      setEliminando(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Gestión de Clientes</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={() => {
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
        <Center py="xl"> <Loader /> </Center>
      ) : clientes.length === 0 ? (
        // Si NO HAY clientes en la base de datos (Empty State real)
        <EmptyState 
          icon={IconUsers}
          title="Sin clientes registrados"
          description="Aún no tenés clientes en tu base de datos. Agregá tu primer cliente para empezar a asignarle turnos."
          actionLabel="Crear mi primer cliente"
          onAction={() => {
            form.reset();
            setClienteEditandoId(null);
            openModal();
          }}
        />
      ) : clientesFiltrados.length === 0 ? (
        // Si el usuario buscó algo y no hubo resultados
        <Text c="dimmed" ta="center" py="xl">No se encontraron resultados para tu búsqueda.</Text>
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
                  {/* Agrupamos los botones de acción para que queden alineados */}
                  <Group gap="xs">
                    <Button 
                      variant="light" 
                      size="xs" 
                      onClick={() => {
                        form.setValues({
                          nombre: c.nombre,
                          apellido: c.apellido,
                          email: c.email ?? '',
                          telefono: c.telefono ?? ''
                        });
                        setClienteEditandoId(c.id);
                        openModal();
                      }}
                    >
                      Editar
                    </Button>
                    
                    {/* Botón de Eliminar */}
                    <ActionIcon 
                      variant="light" 
                      color="red" 
                      onClick={() => setClienteAEliminar(c)}
                      title="Eliminar cliente"
                    >
                      <IconTrash size={16} />
                    </ActionIcon>
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {/* Modal de Creación / Edición */}
      <Modal opened={modalOpened} onClose={() => { closeModal(); setClienteEditandoId(null); }} 
             title={clienteEditandoId ? "Editar cliente" : "Nuevo cliente"} centered>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <TextInput label="Nombre" required {...form.getInputProps('nombre')} />
            <TextInput label="Apellido" required {...form.getInputProps('apellido')} />
            <TextInput label="Teléfono" {...form.getInputProps('telefono')} />
            <TextInput label="Email" {...form.getInputProps('email')} />
            <Button type="submit" loading={submitting} fullWidth mt="sm">
              Guardar
            </Button>
          </Stack>
        </form>
      </Modal>

      {/* Modal de Confirmación de Eliminación */}
      <Modal 
        opened={!!clienteAEliminar} 
        onClose={() => setClienteAEliminar(null)} 
        title={<Text c="red" fw={600}>Eliminar Cliente</Text>} 
        centered
      >
        <Text size="sm">
          ¿Estás seguro que querés eliminar a <strong>{clienteAEliminar?.nombre} {clienteAEliminar?.apellido}</strong>? Esta acción no se puede deshacer.
        </Text>
        
        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={() => setClienteAEliminar(null)} disabled={eliminando}>
            Cancelar
          </Button>
          <Button color="red" onClick={handleEliminar} loading={eliminando}>
            Eliminar
          </Button>
        </Group>
      </Modal>

    </Stack>
  );
}