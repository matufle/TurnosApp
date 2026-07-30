import { useCallback, useEffect, useMemo, useState } from 'react';
import axios from 'axios';
import { Select, Switch } from '@mantine/core';
import { useForm, isNotEmpty } from '@mantine/form';
import { recursosService } from '../../api/recursosService';
import { turnosService } from '../../api/turnosService';
import type { Recurso, UsuarioParaVincular } from '../../types/Recurso';
import type { Turno } from '../../types/Turno';
import { getContrastTextColor } from '../../utils/colorContrast';
import { PageSpinner } from '../../components/PageSpinner';
import { RequirePermission } from '../../auth/RequirePermission';

function iniciales(nombre: string) {
  return nombre
    .split(' ')
    .map((p) => p[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase();
}

export function RecursosPage() {
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [searchTerm, setSearchTerm] = useState('');
  const [modalAbierto, setModalAbierto] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [recursoEditandoId, setRecursoEditandoId] = useState<number | null>(null);

  const [recursoAEliminar, setRecursoAEliminar] = useState<Recurso | null>(null);
  const [eliminando, setEliminando] = useState(false);

  const [usuariosDisponibles, setUsuariosDisponibles] = useState<UsuarioParaVincular[]>([]);
  const [recursoActivoEditando, setRecursoActivoEditando] = useState(true);

  const form = useForm({
    initialValues: { nombre: '', descripcion: '', colorHex: '#0EA5E9', usuarioId: '' },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
    },
  });

  const cargarRecursos = useCallback(async () => {
    try {
      const [recursosData, turnosData] = await Promise.all([recursosService.getAll(), turnosService.getAll()]);
      setRecursos(recursosData);
      setTurnos(turnosData);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los recursos.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarRecursos();
  }, [cargarRecursos]);

  const recursosFiltrados = useMemo(() => {
    const termino = searchTerm.toLowerCase();
    return recursos.filter(
      (r) => r.nombre.toLowerCase().includes(termino) || r.descripcion?.toLowerCase().includes(termino)
    );
  }, [recursos, searchTerm]);

  const ocupadoAhoraPorRecurso = useMemo(() => {
    const ahora = new Date();
    const set = new Set<number>();
    turnos.forEach((t) => {
      if (t.estado === 'Cancelado') return;
      const inicio = new Date(t.fechaHoraInicio);
      const fin = new Date(t.fechaHoraFin);
      if (ahora >= inicio && ahora < fin) {
        set.add(t.recursoId);
      }
    });
    return set;
  }, [turnos]);

  const cargarUsuariosDisponibles = async (recursoIdActual?: number) => {
    try {
      const usuarios = await recursosService.getUsuariosDisponibles(recursoIdActual);
      setUsuariosDisponibles(usuarios);
    } catch {
      setUsuariosDisponibles([]);
    }
  };

  const abrirNuevo = () => {
    form.reset();
    form.setFieldValue('colorHex', '#0EA5E9');
    setRecursoEditandoId(null);
    setRecursoActivoEditando(true);
    setModalAbierto(true);
    void cargarUsuariosDisponibles();
  };

  const abrirEdicion = (r: Recurso) => {
    form.setValues({
      nombre: r.nombre,
      descripcion: r.descripcion || '',
      colorHex: r.colorHex || '#0EA5E9',
      usuarioId: r.usuarioId ? String(r.usuarioId) : '',
    });
    setRecursoEditandoId(r.id);
    setRecursoActivoEditando(r.activo);
    setModalAbierto(true);
    void cargarUsuariosDisponibles(r.id);
  };

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      const usuarioId = values.usuarioId ? Number(values.usuarioId) : null;
      if (recursoEditandoId) {
        await recursosService.update(recursoEditandoId, { ...values, usuarioId, activo: recursoActivoEditando });
      } else {
        await recursosService.create({ ...values, usuarioId });
      }
      setModalAbierto(false);
      form.reset();
      setRecursoEditandoId(null);
      await cargarRecursos();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        form.setFieldError('usuarioId', error.response.data.detail);
      } else {
        setErrorMessage('No pudimos guardar el recurso.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleEliminar = async () => {
    if (!recursoAEliminar) return;
    setEliminando(true);
    try {
      await recursosService.delete(recursoAEliminar.id);
      setRecursos((prev) => prev.filter((r) => r.id !== recursoAEliminar.id));
      setRecursoAEliminar(null);
    } catch {
      setErrorMessage('No pudimos eliminar el recurso. Puede que tenga turnos asociados.');
    } finally {
      setEliminando(false);
    }
  };

  if (loading) {
    return (
      <PageSpinner />
    );
  }

  return (
    <div className="flex flex-col gap-10 pb-12">
      {/* Encabezado */}
      <section className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div className="max-w-2xl">
          <h1 className="font-display-lg text-display-lg text-on-surface mb-3">Gestión de Recursos</h1>
          <p className="font-body-lg text-body-lg text-secondary">
            Administrá tu personal y espacios físicos que se pueden reservar en la agenda.
          </p>
        </div>
        <div className="flex gap-4 w-full md:w-auto">
          <div className="relative grow md:w-64">
            <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">
              search
            </span>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Buscar recurso..."
              className="w-full bg-surface-container-lowest border border-outline-variant rounded-full py-3 pl-12 pr-4 font-body-sm text-body-sm focus:outline-none focus:border-primary-container focus:ring-1 focus:ring-primary-container transition-all"
            />
          </div>
          <RequirePermission permiso="GestionarRecursos">
            <button
              data-tour="recursos-nuevo"
              onClick={abrirNuevo}
              className="shrink-0 bg-primary text-on-primary hover:bg-on-primary-fixed-variant transition-colors px-6 py-3 rounded-full font-label-md text-label-md flex items-center justify-center gap-2 shadow-sm whitespace-nowrap"
            >
              <span className="material-symbols-outlined text-[20px]">add</span>
              Añadir recurso
            </button>
          </RequirePermission>
        </div>
      </section>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      {recursos.length === 0 ? (
        <div className="bg-surface-container-lowest rounded-3xl p-12 soft-elevation flex flex-col items-center text-center gap-3">
          <span className="material-symbols-outlined text-4xl text-outline">badge</span>
          <h3 className="font-title-md text-title-md text-on-surface">Sin recursos configurados</h3>
          <p className="font-body-sm text-body-sm text-secondary max-w-sm">
            Los recursos son los lugares o profesionales que se reservan. Creá el primero para armar tu agenda.
          </p>
          <RequirePermission permiso="GestionarRecursos">
            <button
              onClick={abrirNuevo}
              className="mt-2 bg-primary-container text-on-primary-container font-title-md text-title-md px-6 py-3 rounded-full hover:bg-primary transition-colors"
            >
              Agregar recurso
            </button>
          </RequirePermission>
        </div>
      ) : recursosFiltrados.length === 0 ? (
        <p className="font-body-lg text-body-lg text-secondary text-center py-8">
          No se encontraron resultados para tu búsqueda.
        </p>
      ) : (
        <section className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter">
          {recursosFiltrados.map((r) => {
            const color = r.colorHex || '#0EA5E9';
            const ocupado = ocupadoAhoraPorRecurso.has(r.id);
            return (
              <div
                key={r.id}
                className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation flex flex-col gap-6 relative overflow-hidden hover:-translate-y-1 transition-transform duration-300"
              >
                <div className="absolute top-0 left-0 w-2 h-full rounded-l-3xl" style={{ backgroundColor: color }} />

                <div className="flex justify-between items-start gap-3">
                  <div className="flex items-center gap-4 min-w-0">
                    <div
                      className="w-12 h-12 rounded-full flex items-center justify-center font-title-md text-title-md shrink-0"
                      style={{ backgroundColor: color, color: getContrastTextColor(color) }}
                    >
                      {iniciales(r.nombre)}
                    </div>
                    <div className="min-w-0">
                      <h3 className="font-title-md text-title-md text-on-surface truncate">{r.nombre}</h3>
                      <p className="font-body-sm text-body-sm text-secondary truncate">
                        {r.descripcion || 'Sin descripción'}
                      </p>
                      {r.usuarioNombre && (
                        <p className="font-label-md text-[10px] text-primary uppercase tracking-wider truncate mt-0.5 flex items-center gap-1">
                          <span className="material-symbols-outlined text-[12px]">link</span>
                          {r.usuarioNombre}
                        </p>
                      )}
                    </div>
                  </div>
                  {r.activo ? (
                    <span
                      className={`px-3 py-1 rounded-full font-label-md text-[10px] uppercase tracking-wider flex items-center gap-1 shrink-0 ${
                        ocupado ? 'bg-error-container text-error' : 'bg-secondary-container text-primary'
                      }`}
                    >
                      <span className={`w-1.5 h-1.5 rounded-full ${ocupado ? 'bg-error' : 'bg-primary'}`} />
                      {ocupado ? 'Ocupado ahora' : 'Disponible ahora'}
                    </span>
                  ) : (
                    <span className="px-3 py-1 rounded-full font-label-md text-[10px] uppercase tracking-wider flex items-center gap-1 shrink-0 bg-surface-container-high text-secondary">
                      <span className="w-1.5 h-1.5 rounded-full bg-secondary" />
                      Inactivo
                    </span>
                  )}
                </div>

                <RequirePermission permiso="GestionarRecursos">
                  <div className="mt-auto pt-4 border-t border-outline-variant/30 flex gap-2">
                    <button
                      onClick={() => abrirEdicion(r)}
                      className="flex-1 bg-surface text-primary border border-outline-variant hover:border-primary transition-colors py-2 rounded-full font-label-md text-label-md"
                    >
                      Editar
                    </button>
                    <button
                      aria-label="Eliminar"
                      onClick={() => setRecursoAEliminar(r)}
                      className="p-2 text-secondary hover:text-error hover:border-error transition-colors bg-surface border border-outline-variant rounded-full flex items-center justify-center"
                    >
                      <span className="material-symbols-outlined text-[20px]">delete</span>
                    </button>
                  </div>
                </RequirePermission>
              </div>
            );
          })}

          <RequirePermission permiso="GestionarRecursos">
            <button
              onClick={abrirNuevo}
              className="bg-surface-bright border-2 border-dashed border-outline-variant hover:border-primary transition-colors rounded-3xl p-6 flex flex-col items-center justify-center gap-4 min-h-[220px] group"
            >
              <div className="w-16 h-16 rounded-full bg-surface-container-low group-hover:bg-primary-container transition-colors flex items-center justify-center text-primary group-hover:text-on-primary-container">
                <span className="material-symbols-outlined text-[32px]">add</span>
              </div>
              <div className="text-center">
                <h3 className="font-title-md text-title-md text-on-surface group-hover:text-primary transition-colors">
                  Añadir nuevo recurso
                </h3>
                <p className="font-body-sm text-body-sm text-secondary mt-1">Personal, salas o equipos</p>
              </div>
            </button>
          </RequirePermission>
        </section>
      )}

      {/* Modal de alta / edición */}
      {modalAbierto && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-on-background/40 backdrop-blur-sm px-4">
          <div className="bg-surface-container-lowest rounded-3xl p-8 soft-elevation max-w-md w-full max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-6">
              <h2 className="font-title-md text-title-md text-on-surface">
                {recursoEditandoId ? 'Editar recurso' : 'Nuevo recurso'}
              </h2>
              <button
                onClick={() => setModalAbierto(false)}
                className="text-secondary hover:text-on-surface transition-colors"
                aria-label="Cerrar"
              >
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>

            <form className="flex flex-col gap-4" onSubmit={form.onSubmit(handleSubmit)} noValidate>
              <div>
                <label className="block font-label-md text-label-md text-secondary mb-1">Nombre</label>
                <input
                  type="text"
                  placeholder="Ej: Dra. Ana Silva"
                  className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                  {...form.getInputProps('nombre')}
                />
                {form.errors.nombre && (
                  <p className="font-body-sm text-body-sm text-error mt-1">{form.errors.nombre}</p>
                )}
              </div>

              <div>
                <label className="block font-label-md text-label-md text-secondary mb-1">Descripción</label>
                <input
                  type="text"
                  placeholder="Ej: Especialista general, Sala 3..."
                  className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                  {...form.getInputProps('descripcion')}
                />
              </div>

              <div>
                <label className="block font-label-md text-label-md text-secondary mb-1">Color en la agenda</label>
                <div className="flex items-center gap-3">
                  <input
                    type="color"
                    className="w-12 h-12 rounded-xl border border-outline-variant cursor-pointer bg-surface-bright p-1"
                    {...form.getInputProps('colorHex')}
                  />
                  <input
                    type="text"
                    className="flex-1 bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all uppercase"
                    {...form.getInputProps('colorHex')}
                  />
                </div>
              </div>

              <div>
                <label className="block font-label-md text-label-md text-secondary mb-1">
                  Vincular a un usuario (opcional)
                </label>
                <Select
                  placeholder="Sin usuario vinculado"
                  clearable
                  data={usuariosDisponibles.map((u) => ({ value: String(u.id), label: u.nombre }))}
                  {...form.getInputProps('usuarioId')}
                />
                <p className="font-body-sm text-body-sm text-secondary mt-1">
                  Si esta persona también inicia sesión en Turnify, vinculá su usuario para no duplicar la carga.
                </p>
              </div>

              {recursoEditandoId && (
                <Switch
                  label="Recurso activo"
                  description="Si lo desactivás, deja de aparecer para reservar (incluida la reserva pública)."
                  checked={recursoActivoEditando}
                  onChange={(e) => setRecursoActivoEditando(e.currentTarget.checked)}
                />
              )}

              <button
                type="submit"
                disabled={submitting}
                className="w-full bg-primary text-on-primary font-title-md text-title-md py-3 rounded-xl hover:bg-primary-container hover:text-on-primary-container transition-colors mt-2 disabled:opacity-70 flex items-center justify-center gap-2"
              >
                {submitting ? (
                  <span className="material-symbols-outlined animate-spin text-[20px]">progress_activity</span>
                ) : null}
                Guardar
              </button>
            </form>
          </div>
        </div>
      )}

      {/* Confirmación de borrado */}
      {recursoAEliminar && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-on-background/40 backdrop-blur-sm px-4">
          <div className="bg-surface-container-lowest rounded-3xl p-8 soft-elevation max-w-sm w-full">
            <h3 className="font-title-md text-title-md text-error mb-2">Eliminar recurso</h3>
            <p className="font-body-lg text-body-lg text-secondary">
              ¿Estás seguro que querés eliminar{' '}
              <strong className="text-on-surface">{recursoAEliminar.nombre}</strong>? Esta acción no se puede
              deshacer.
            </p>
            <div className="flex justify-end gap-3 mt-6">
              <button
                onClick={() => setRecursoAEliminar(null)}
                disabled={eliminando}
                className="px-5 py-2 rounded-full font-title-md text-title-md text-secondary hover:bg-surface-container transition-colors disabled:opacity-60"
              >
                Cancelar
              </button>
              <button
                onClick={handleEliminar}
                disabled={eliminando}
                className="px-5 py-2 rounded-full font-title-md text-title-md bg-error text-on-error hover:opacity-90 transition-opacity disabled:opacity-60 flex items-center gap-2"
              >
                {eliminando ? (
                  <span className="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
                ) : null}
                Eliminar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
