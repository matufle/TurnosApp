// src/pages/Servicios/ServiciosPage.tsx
import { useEffect, useState, useMemo, useCallback } from 'react';
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
  ActionIcon,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconPlus, IconAlertCircle, IconSearch, IconTrash, IconBriefcase } from '@tabler/icons-react';
import { serviciosService } from '../../api/servicioService';
import type { Servicio } from '../../types/Servicio';
import { EmptyState } from '../../components/EmptyState'; // Importamos el componente

export function ServiciosPage() {
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  
  const [searchTerm, setSearchTerm] = useState('');
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [submitting, setSubmitting] = useState(false);
  const [servicioEditandoId, setServicioEditandoId] = useState<number | null>(null);

  const [servicioAEliminar, setServicioAEliminar] = useState<Servicio | null>(null);
  const [eliminando, setEliminando] = useState(false);

  const form = useForm({
    initialValues: { nombre: '', descripcion: '', duracionMinutos: 30, precio: 0 },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      duracionMinutos: (value) => (value > 0 ? null : 'La duración debe ser mayor a 0'),
      precio: (value) => (value >= 0 ? null : 'El precio no puede ser negativo'),
    },
  });

  const serviciosFiltrados = useMemo(() => {
    return servicios.filter((s) => 
      s.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (s.descripcion?.toLowerCase().includes(searchTerm.toLowerCase()))
    );
  }, [servicios, searchTerm]);

  const cargarServicios = useCallback(async () => {
    setLoading(true);
    try {
      const data = await serviciosService.getAll();
      setServicios(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los servicios.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const cargar = async () => {
      setLoading(true);
      try {
        const data = await serviciosService.getAll();
        setServicios(data);
        setErrorMessage(null);
      } catch {
        setErrorMessage('No pudimos cargar los servicios.');
      } finally {
        setLoading(false);
      }
    };

    cargar();
  }, []);

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      if (servicioEditandoId) {
        await serviciosService.update(servicioEditandoId, values);
      } else {
        await serviciosService.create(values);
      }
      closeModal();
      form.reset();
      setServicioEditandoId(null);
      await cargarServicios();
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

  const handleEliminar = async () => {
    if (!servicioAEliminar) return;
    setEliminando(true);
    try {
      await serviciosService.delete(servicioAEliminar.id);
      
      setServicios((prev) => prev.filter((s) => s.id !== servicioAEliminar.id));
      setServicioAEliminar(null);
    } catch {
      setErrorMessage('No pudimos eliminar el servicio. Puede que tenga turnos asociados.');
    } finally {
      setEliminando(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Gestión de Servicios</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={() => {
          form.reset();
          setServicioEditandoId(null);
          openModal();
        }}>
          Nuevo servicio
        </Button>
      </Group>

      <TextInput
        placeholder="Buscar por nombre o descripción..."
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
      ) : servicios.length === 0 ? (
        <EmptyState 
          icon={IconBriefcase}
          title="Sin servicios creados"
          description="Definí qué servicios ofrecés, su duración y su precio para que los clientes puedan reservar."
          actionLabel="Crear nuevo servicio"
          onAction={() => {
            form.reset();
            setServicioEditandoId(null);
            openModal();
          }}
        />
      ) : serviciosFiltrados.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">No se encontraron resultados para tu búsqueda.</Text>
      ) : (
        <Table striped highlightOnHover verticalSpacing="sm">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Nombre</Table.Th>
              <Table.Th>Descripción</Table.Th>
              <Table.Th>Duración</Table.Th>
              <Table.Th>Precio</Table.Th>
              <Table.Th>Acciones</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {serviciosFiltrados.map((s) => (
              <Table.Tr key={s.id}>
                <Table.Td>{s.nombre}</Table.Td>
                <Table.Td>{s.descripcion ?? '—'}</Table.Td>
                <Table.Td>{s.duracionMinutos} min</Table.Td>
                <Table.Td>${s.precio}</Table.Td>
                <Table.Td>
                  <Group gap="xs">
                    <Button variant="light" size="xs" onClick={() => {
                      form.setValues(s);
                      setServicioEditandoId(s.id);
                      openModal();
                    }}>
                      Editar
                    </Button>
                    
                    <ActionIcon 
                      variant="light" 
                      color="red" 
                      onClick={() => setServicioAEliminar(s)}
                      title="Eliminar servicio"
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

      <Modal opened={modalOpened} onClose={() => { closeModal(); setServicioEditandoId(null); }} 
             title={servicioEditandoId ? "Editar servicio" : "Nuevo servicio"} centered>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <TextInput label="Nombre" required {...form.getInputProps('nombre')} />
            <Textarea label="Descripción" {...form.getInputProps('descripcion')} />
            <NumberInput label="Duración (minutos)" min={1} required {...form.getInputProps('duracionMinutos')} />
            <NumberInput label="Precio" min={0} prefix="$" required {...form.getInputProps('precio')} />
            <Button type="submit" loading={submitting} fullWidth mt="sm">
              Guardar
            </Button>
          </Stack>
        </form>
      </Modal>

      <Modal 
        opened={!!servicioAEliminar} 
        onClose={() => setServicioAEliminar(null)} 
        title={<Text c="red" fw={600}>Eliminar Servicio</Text>} 
        centered
      >
        <Text size="sm">
          ¿Estás seguro que querés eliminar el servicio <strong>{servicioAEliminar?.nombre}</strong>? Esta acción no se puede deshacer.
        </Text>
        
        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={() => setServicioAEliminar(null)} disabled={eliminando}>
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