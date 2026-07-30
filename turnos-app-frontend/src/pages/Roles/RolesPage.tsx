// src/pages/Roles/RolesPage.tsx
import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import { Modal, TextInput, Checkbox, Button, Menu, ActionIcon, Badge, Stack, Text } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconDots, IconEdit, IconTrash, IconShieldLock } from '@tabler/icons-react';
import { rolesService } from '../../api/rolesService';
import { EmptyState } from '../../components/EmptyState';
import { PageSpinner } from '../../components/PageSpinner';
import { PERMISOS_POR_AREA, type Permiso } from '../../types/Permiso';
import type { Rol } from '../../types/Rol';

interface RolFormValues {
  nombre: string;
  permisos: Permiso[];
}

export function RolesPage() {
  const [roles, setRoles] = useState<Rol[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [rolEditando, setRolEditando] = useState<Rol | null>(null);
  const [modalOpened, { open, close }] = useDisclosure(false);

  const form = useForm<RolFormValues>({
    initialValues: { nombre: '', permisos: [] },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
    },
  });

  const cargarRoles = useCallback(async () => {
    try {
      const data = await rolesService.getAll();
      setRoles(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los roles.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarRoles();
  }, [cargarRoles]);

  const abrirCrear = () => {
    form.reset();
    setRolEditando(null);
    open();
  };

  const abrirEditar = (rol: Rol) => {
    form.setValues({ nombre: rol.nombre, permisos: rol.permisos });
    setRolEditando(rol);
    open();
  };

  const handleSubmit = async (values: RolFormValues) => {
    setSubmitting(true);
    try {
      if (rolEditando) {
        await rolesService.update(rolEditando.id, values);
      } else {
        await rolesService.create(values);
      }
      close();
      form.reset();
      setRolEditando(null);
      await cargarRoles();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos guardar los cambios.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (rol: Rol) => {
    try {
      await rolesService.delete(rol.id);
      await cargarRoles();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos eliminar el rol.');
      }
    }
  };

  if (loading) {
    return (
      <PageSpinner />
    );
  }

  return (
    <div className="flex flex-col gap-10 pb-12">
      <header className="flex justify-between items-center">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface">Roles y Permisos</h1>
          <p className="font-body-lg text-body-lg text-secondary mt-2">
            Creá roles a medida y elegí qué puede ver y hacer cada uno.
          </p>
        </div>
        <button
          data-tour="roles-nuevo"
          onClick={abrirCrear}
          className="bg-primary text-on-primary font-label-md text-label-md px-6 py-3 rounded-full soft-elevation hover:bg-primary-container hover:shadow-lg transition-all duration-200 flex items-center gap-2 whitespace-nowrap"
        >
          <span className="material-symbols-outlined">add</span> Nuevo rol
        </button>
      </header>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      {roles.length === 0 ? (
        <EmptyState
          title="Sin roles"
          description="Creá roles a medida para definir qué puede ver y hacer cada miembro de tu equipo."
          icon={IconShieldLock}
          actionLabel="Nuevo rol"
          onAction={abrirCrear}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter">
          {roles.map((rol) => (
            <div
              key={rol.id}
              className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation flex flex-col gap-4"
            >
              <div className="flex justify-between items-start">
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-title-md text-title-md text-on-surface">{rol.nombre}</h3>
                    {rol.esSistema && (
                      <Badge size="sm" variant="light" color="gray">
                        Sistema
                      </Badge>
                    )}
                  </div>
                  <p className="font-body-sm text-body-sm text-secondary mt-1">
                    {rol.permisos.length} {rol.permisos.length === 1 ? 'permiso' : 'permisos'}
                  </p>
                </div>

                {!rol.esSistema && (
                  <Menu shadow="md" position="bottom-end" withinPortal>
                    <Menu.Target>
                      <ActionIcon variant="subtle" color="gray" aria-label="Más opciones">
                        <IconDots size={18} />
                      </ActionIcon>
                    </Menu.Target>
                    <Menu.Dropdown>
                      <Menu.Item leftSection={<IconEdit size={16} />} onClick={() => abrirEditar(rol)}>
                        Editar
                      </Menu.Item>
                      <Menu.Item leftSection={<IconTrash size={16} />} color="red" onClick={() => handleDelete(rol)}>
                        Eliminar
                      </Menu.Item>
                    </Menu.Dropdown>
                  </Menu>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal
        opened={modalOpened}
        onClose={() => {
          close();
          setRolEditando(null);
        }}
        title={rolEditando ? 'Editar rol' : 'Nuevo rol'}
        centered
        size="lg"
      >
        <form onSubmit={form.onSubmit(handleSubmit)} className="flex flex-col gap-4">
          <TextInput label="Nombre" placeholder="Ej: Supervisor" required {...form.getInputProps('nombre')} />

          <Checkbox.Group
            label="Permisos"
            value={form.values.permisos}
            onChange={(value) => form.setFieldValue('permisos', value as Permiso[])}
          >
            <Stack gap="lg" mt="xs">
              {PERMISOS_POR_AREA.map((grupo) => (
                <div key={grupo.area}>
                  <Text size="sm" fw={600} c="dimmed" mb="xs">
                    {grupo.area}
                  </Text>
                  <Stack gap="xs">
                    {grupo.permisos.map((permiso) => (
                      <Checkbox key={permiso.valor} value={permiso.valor} label={permiso.label} />
                    ))}
                  </Stack>
                </div>
              ))}
            </Stack>
          </Checkbox.Group>

          <Button type="submit" loading={submitting} fullWidth mt="sm">
            {rolEditando ? 'Guardar cambios' : 'Crear rol'}
          </Button>
        </form>
      </Modal>
    </div>
  );
}
