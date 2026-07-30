// src/pages/Usuarios/UsuariosPage.tsx
import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import { Modal, TextInput, PasswordInput, Select, Button, Menu, ActionIcon } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isEmail, isNotEmpty } from '@mantine/form';
import { IconDots, IconEdit, IconBan, IconRefresh, IconUsersGroup } from '@tabler/icons-react';
import { usuariosService } from '../../api/usuariosService';
import { rolesService } from '../../api/rolesService';
import { EmptyState } from '../../components/EmptyState';
import { PageSpinner } from '../../components/PageSpinner';
import type { Usuario } from '../../types/Usuario';
import type { Rol } from '../../types/Rol';

interface UsuarioFormValues {
  nombre: string;
  email: string;
  password: string;
  rolId: string | null;
}

export function UsuariosPage() {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [roles, setRoles] = useState<Rol[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [usuarioEditando, setUsuarioEditando] = useState<Usuario | null>(null);
  const [modalOpened, { open, close }] = useDisclosure(false);

  const form = useForm<UsuarioFormValues>({
    initialValues: { nombre: '', email: '', password: '', rolId: null },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      email: (value) => (usuarioEditando ? null : isEmail('Ingresá un email válido')(value)),
      password: (value) => (usuarioEditando || value.length >= 8 ? null : 'La contraseña debe tener al menos 8 caracteres'),
      rolId: isNotEmpty('Elegí un rol'),
    },
  });

  const cargarDatos = useCallback(async () => {
    try {
      const [usuariosData, rolesData] = await Promise.all([usuariosService.getAll(), rolesService.getAll()]);
      setUsuarios(usuariosData);
      setRoles(rolesData);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los usuarios.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarDatos();
  }, [cargarDatos]);

  const rolesData = roles.map((rol) => ({ value: String(rol.id), label: rol.nombre }));

  const abrirCrear = () => {
    form.reset();
    setUsuarioEditando(null);
    open();
  };

  const abrirEditar = (usuario: Usuario) => {
    form.setValues({ nombre: usuario.nombre, email: usuario.email, password: '', rolId: String(usuario.rolId) });
    setUsuarioEditando(usuario);
    open();
  };

  const handleSubmit = async (values: UsuarioFormValues) => {
    if (!values.rolId) return;
    setSubmitting(true);
    try {
      if (usuarioEditando) {
        await usuariosService.update(usuarioEditando.id, { nombre: values.nombre, rolId: Number(values.rolId) });
      } else {
        await usuariosService.create({
          nombre: values.nombre,
          email: values.email,
          password: values.password,
          rolId: Number(values.rolId),
        });
      }
      close();
      form.reset();
      setUsuarioEditando(null);
      await cargarDatos();
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

  const handleDesactivar = async (usuario: Usuario) => {
    try {
      const actualizado = await usuariosService.desactivar(usuario.id);
      setUsuarios((prev) => prev.map((u) => (u.id === actualizado.id ? actualizado : u)));
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos desactivar el usuario.');
      }
    }
  };

  const handleActivar = async (usuario: Usuario) => {
    try {
      const actualizado = await usuariosService.activar(usuario.id);
      setUsuarios((prev) => prev.map((u) => (u.id === actualizado.id ? actualizado : u)));
    } catch {
      setErrorMessage('No pudimos reactivar el usuario.');
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
          <h1 className="font-headline-lg text-headline-lg text-on-surface">Usuarios</h1>
          <p className="font-body-lg text-body-lg text-secondary mt-2">
            Gestioná quién puede acceder a tu negocio y con qué rol.
          </p>
        </div>
        <button
          data-tour="usuarios-nuevo"
          onClick={abrirCrear}
          className="bg-primary text-on-primary font-label-md text-label-md px-6 py-3 rounded-full soft-elevation hover:bg-primary-container hover:shadow-lg transition-all duration-200 flex items-center gap-2 whitespace-nowrap"
        >
          <span className="material-symbols-outlined">add</span> Nuevo usuario
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

      {usuarios.length === 0 ? (
        <EmptyState
          title="Sin usuarios"
          description="Creá usuarios para que tu equipo pueda acceder al sistema con el rol que le corresponde."
          icon={IconUsersGroup}
          actionLabel="Nuevo usuario"
          onAction={abrirCrear}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter">
          {usuarios.map((usuario) => (
            <div
              key={usuario.id}
              className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation flex flex-col gap-4 relative overflow-hidden"
            >
              <div className={`absolute left-0 top-0 bottom-0 w-2 ${usuario.activo ? 'bg-primary' : 'bg-outline-variant'}`} />

              <div className={`flex justify-between items-start ml-2 ${usuario.activo ? '' : 'opacity-60'}`}>
                <div>
                  <h3 className="font-title-md text-title-md text-on-surface">{usuario.nombre}</h3>
                  <p className="font-body-sm text-body-sm text-secondary">{usuario.email}</p>
                  <div className="flex items-center gap-2 mt-2">
                    <span className="inline-flex items-center px-2 py-0.5 rounded-full font-label-md text-[10px] bg-secondary-container text-on-secondary-container">
                      {usuario.rolNombre}
                    </span>
                    <span
                      className={`inline-flex items-center px-2 py-0.5 rounded-full font-label-md text-[10px] ${
                        usuario.activo
                          ? 'bg-secondary-container text-on-secondary-container'
                          : 'bg-surface-variant text-on-surface-variant'
                      }`}
                    >
                      {usuario.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </div>
                </div>

                <Menu shadow="md" position="bottom-end" withinPortal>
                  <Menu.Target>
                    <ActionIcon variant="subtle" color="gray" aria-label="Más opciones">
                      <IconDots size={18} />
                    </ActionIcon>
                  </Menu.Target>
                  <Menu.Dropdown>
                    <Menu.Item leftSection={<IconEdit size={16} />} onClick={() => abrirEditar(usuario)}>
                      Editar
                    </Menu.Item>
                    {usuario.activo ? (
                      <Menu.Item leftSection={<IconBan size={16} />} color="red" onClick={() => handleDesactivar(usuario)}>
                        Desactivar
                      </Menu.Item>
                    ) : (
                      <Menu.Item leftSection={<IconRefresh size={16} />} onClick={() => handleActivar(usuario)}>
                        Reactivar
                      </Menu.Item>
                    )}
                  </Menu.Dropdown>
                </Menu>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal
        opened={modalOpened}
        onClose={() => {
          close();
          setUsuarioEditando(null);
        }}
        title={usuarioEditando ? 'Editar usuario' : 'Nuevo usuario'}
        centered
      >
        <form onSubmit={form.onSubmit(handleSubmit)} className="flex flex-col gap-4">
          <TextInput label="Nombre" placeholder="Ej: Juan Pérez" required {...form.getInputProps('nombre')} />

          <TextInput
            label="Email"
            placeholder="usuario@negocio.com"
            required
            disabled={Boolean(usuarioEditando)}
            description={usuarioEditando ? 'El email no se puede modificar' : undefined}
            {...form.getInputProps('email')}
          />

          {!usuarioEditando && (
            <PasswordInput
              label="Contraseña"
              placeholder="Mínimo 8 caracteres"
              required
              {...form.getInputProps('password')}
            />
          )}

          <Select
            label="Rol"
            placeholder="Elegí un rol"
            data={rolesData}
            allowDeselect={false}
            {...form.getInputProps('rolId')}
          />

          <Button type="submit" loading={submitting} fullWidth mt="sm">
            {usuarioEditando ? 'Guardar cambios' : 'Crear usuario'}
          </Button>
        </form>
      </Modal>
    </div>
  );
}
