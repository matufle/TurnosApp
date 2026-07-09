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
  ColorInput,
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
  
  // Estado para saber si estamos editando
  const [recursoEditandoId, setRecursoEditandoId] = useState<number | null>(null);

  const form = useForm({
    initialValues: { nombre: '', descripcion: '', colorHex: '#0EA5E9' },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
    },
  });

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
      if (recursoEditandoId) {
        // Modo Edición
        await recursosService.update(recursoEditandoId, values);
      } else {
        // Modo Creación
        await recursosService.create(values);
      }
      
      closeModal();
      form.reset();
      setRecursoEditandoId(null);
      await recargarRecursos();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        form.setFieldError('nombre', error.response.data.detail);
      } else {
        setErrorMessage('No pudimos guardar el recurso. Intentá de nuevo.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={2}>Gestión de Recursos</Title>
        <Button 
          leftSection={<IconPlus size={16} />} 
          color="cyan" 
          onClick={() => {
            form.reset();
            form.setFieldValue('colorHex', '#0EA5E9');
            setRecursoEditandoId(null);
            openModal();
          }}
        >
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
              <Table.Th>Acciones</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {recursos.map((recurso) => (
              <Table.Tr key={recurso.id}>
                <Table.Td>{recurso.nombre}</Table.Td>
                <Table.Td>{recurso.descripcion}</Table.Td>
                <Table.Td>
                  <Button 
                    variant="light" 
                    color="cyan" 
                    size="xs"
                    onClick={() => {
                      form.setValues({
                        nombre: recurso.nombre,
                        descripcion: recurso.descripcion || '',
                        colorHex: recurso.colorHex || '#0EA5E9',
                      });
                      setRecursoEditandoId(recurso.id);
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

      <Modal 
        opened={modalOpened} 
        onClose={() => {
          closeModal();
          setRecursoEditandoId(null);
        }} 
        title={recursoEditandoId ? "Editar recurso" : "Nuevo recurso"} 
        centered
      >
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

            <ColorInput
              label="Color en la agenda"
              placeholder="Elegí un color"
              format="hex"
              swatches={['#0EA5E9', '#12b886', '#fab005', '#fd7e14', '#fa5252', '#be4bdb', '#7950f2']}
              {...form.getInputProps('colorHex')}
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