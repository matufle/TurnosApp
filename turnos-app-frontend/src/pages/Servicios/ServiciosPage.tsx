import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import axios from 'axios';
import { useForm, isNotEmpty } from '@mantine/form';
import { serviciosService } from '../../api/servicioService';
import type { Servicio } from '../../types/Servicio';

export function ServiciosPage() {
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [servicioEditandoId, setServicioEditandoId] = useState<number | null>(null);
  const [servicioAEliminar, setServicioAEliminar] = useState<Servicio | null>(null);
  const [eliminando, setEliminando] = useState(false);

  const formRef = useRef<HTMLDivElement>(null);
  const nombreInputRef = useRef<HTMLInputElement>(null);

  const form = useForm({
    initialValues: { nombre: '', descripcion: '', duracionMinutos: 30, precio: 0 },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      duracionMinutos: (value) => (value > 0 ? null : 'La duración debe ser mayor a 0'),
      precio: (value) => (value >= 0 ? null : 'El precio no puede ser negativo'),
    },
  });

  const serviciosFiltrados = useMemo(() => {
    return servicios.filter(
      (s) =>
        s.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
        s.descripcion?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [servicios, searchTerm]);

  const cargarServicios = useCallback(async () => {
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
    cargarServicios();
  }, [cargarServicios]);

  const irAlFormulario = () => {
    formRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    nombreInputRef.current?.focus();
  };

  const iniciarEdicion = (s: Servicio) => {
    form.setValues({
      nombre: s.nombre,
      descripcion: s.descripcion ?? '',
      duracionMinutos: s.duracionMinutos,
      precio: s.precio,
    });
    setServicioEditandoId(s.id);
    irAlFormulario();
  };

  const cancelarEdicion = () => {
    form.reset();
    setServicioEditandoId(null);
  };

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      if (servicioEditandoId) {
        await serviciosService.update(servicioEditandoId, values);
      } else {
        await serviciosService.create(values);
      }
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

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <span className="material-symbols-outlined animate-spin text-primary text-4xl">progress_activity</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-10 pb-12">
      {/* Encabezado */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div>
          <h1 className="font-display-lg text-display-lg text-on-surface mb-2">Configuración de Servicios</h1>
          <p className="font-body-lg text-body-lg text-secondary max-w-2xl">
            Gestioná tu catálogo de servicios. Mantené las cosas simples y organizadas para tus clientes.
          </p>
        </div>
        <button
          data-tour="servicios-nuevo"
          onClick={irAlFormulario}
          className="bg-primary-container text-on-primary-container font-title-md text-title-md px-6 py-3 rounded-full hover:bg-primary transition-colors soft-elevation flex items-center gap-2 whitespace-nowrap"
        >
          <span className="material-symbols-outlined">add</span>
          Añadir servicio
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

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-gutter items-start">
        {/* Listado */}
        <section className="lg:col-span-8 flex flex-col gap-6">
          <div className="relative max-w-md">
            <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">
              search
            </span>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Buscar por nombre o descripción..."
              className="w-full bg-surface-container-lowest border border-outline-variant rounded-xl py-3 pl-12 pr-4 font-body-lg text-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary-container/10 transition-all"
            />
          </div>

          {servicios.length === 0 ? (
            <div className="bg-surface-container-lowest rounded-3xl p-12 soft-elevation flex flex-col items-center text-center gap-3">
              <span className="material-symbols-outlined text-4xl text-outline">design_services</span>
              <h3 className="font-title-md text-title-md text-on-surface">Sin servicios creados</h3>
              <p className="font-body-sm text-body-sm text-secondary max-w-sm">
                Definí qué servicios ofrecés, su duración y su precio para que puedan agendarse turnos.
              </p>
              <button
                onClick={irAlFormulario}
                className="mt-2 bg-primary-container text-on-primary-container font-title-md text-title-md px-6 py-3 rounded-full hover:bg-primary transition-colors"
              >
                Crear el primer servicio
              </button>
            </div>
          ) : serviciosFiltrados.length === 0 ? (
            <p className="font-body-lg text-body-lg text-secondary text-center py-8">
              No se encontraron resultados para tu búsqueda.
            </p>
          ) : (
            serviciosFiltrados.map((s) => (
              <div
                key={s.id}
                className={`bg-surface-container-lowest rounded-3xl p-6 soft-elevation flex flex-col sm:flex-row gap-6 items-start sm:items-center relative overflow-hidden group ${
                  servicioEditandoId === s.id ? 'ring-2 ring-primary-container' : ''
                }`}
              >
                <div className="absolute left-0 top-0 bottom-0 w-2 bg-primary" />
                <div className="w-16 h-16 rounded-2xl bg-surface-container-low flex items-center justify-center text-primary shrink-0">
                  <span className="material-symbols-outlined text-3xl">design_services</span>
                </div>
                <div className="grow min-w-0">
                  <h3 className="font-title-md text-title-md text-on-surface">{s.nombre}</h3>
                  {s.descripcion && (
                    <p className="font-body-sm text-body-sm text-secondary mt-1">{s.descripcion}</p>
                  )}
                  <div className="flex flex-wrap gap-3 mt-4">
                    <span className="inline-flex items-center gap-1 px-3 py-1 bg-surface-container-low rounded-full font-label-md text-label-md text-on-secondary-container">
                      <span className="material-symbols-outlined text-[16px]">schedule</span>
                      {s.duracionMinutos} min
                    </span>
                    <span className="inline-flex items-center gap-1 px-3 py-1 bg-surface-container-low rounded-full font-label-md text-label-md text-on-secondary-container">
                      <span className="material-symbols-outlined text-[16px]">payments</span>${s.precio}
                    </span>
                  </div>
                </div>
                <div className="flex sm:flex-col gap-2 w-full sm:w-auto mt-4 sm:mt-0 opacity-100 sm:opacity-0 group-hover:opacity-100 focus-within:opacity-100 transition-opacity duration-200">
                  <button
                    aria-label="Editar"
                    onClick={() => iniciarEdicion(s)}
                    className="flex-1 sm:flex-none p-2 rounded-xl text-secondary hover:bg-surface-container hover:text-primary transition-colors flex justify-center"
                  >
                    <span className="material-symbols-outlined">edit</span>
                  </button>
                  <button
                    aria-label="Eliminar"
                    onClick={() => setServicioAEliminar(s)}
                    className="flex-1 sm:flex-none p-2 rounded-xl text-secondary hover:bg-error-container hover:text-error transition-colors flex justify-center"
                  >
                    <span className="material-symbols-outlined">delete</span>
                  </button>
                </div>
              </div>
            ))
          )}
        </section>

        {/* Formulario (alta / edición) */}
        <aside className="lg:col-span-4" ref={formRef}>
          <div className="bg-surface-container-lowest rounded-3xl p-8 soft-elevation lg:sticky lg:top-6">
            <h2 className="font-title-md text-title-md text-on-surface mb-6 flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">
                {servicioEditandoId ? 'edit' : 'bolt'}
              </span>
              {servicioEditandoId ? 'Editar servicio' : 'Creación rápida'}
            </h2>

            <form className="flex flex-col gap-5" onSubmit={form.onSubmit(handleSubmit)} noValidate>
              <div>
                <label htmlFor="service-name" className="block font-label-md text-label-md text-secondary mb-1">
                  Nombre del servicio
                </label>
                <input
                  id="service-name"
                  ref={nombreInputRef}
                  type="text"
                  placeholder="Ej. Corte de pelo"
                  className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                  {...form.getInputProps('nombre')}
                />
                {form.errors.nombre && (
                  <p className="font-body-sm text-body-sm text-error mt-1">{form.errors.nombre}</p>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label htmlFor="service-duration" className="block font-label-md text-label-md text-secondary mb-1">
                    Duración (min)
                  </label>
                  <input
                    id="service-duration"
                    type="number"
                    min={1}
                    placeholder="45"
                    className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                    {...form.getInputProps('duracionMinutos')}
                  />
                  {form.errors.duracionMinutos && (
                    <p className="font-body-sm text-body-sm text-error mt-1">{form.errors.duracionMinutos}</p>
                  )}
                </div>
                <div>
                  <label htmlFor="service-price" className="block font-label-md text-label-md text-secondary mb-1">
                    Precio ($)
                  </label>
                  <input
                    id="service-price"
                    type="number"
                    min={0}
                    placeholder="100"
                    className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                    {...form.getInputProps('precio')}
                  />
                  {form.errors.precio && (
                    <p className="font-body-sm text-body-sm text-error mt-1">{form.errors.precio}</p>
                  )}
                </div>
              </div>

              <div>
                <label htmlFor="service-desc" className="block font-label-md text-label-md text-secondary mb-1">
                  Descripción breve
                </label>
                <textarea
                  id="service-desc"
                  rows={3}
                  placeholder="Opcional..."
                  className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all resize-none"
                  {...form.getInputProps('descripcion')}
                />
              </div>

              <button
                type="submit"
                disabled={submitting}
                className="w-full bg-primary text-on-primary font-title-md text-title-md py-3 rounded-xl hover:bg-primary-container hover:text-on-primary-container transition-colors mt-2 disabled:opacity-70 flex items-center justify-center gap-2"
              >
                {submitting ? (
                  <span className="material-symbols-outlined animate-spin text-[20px]">progress_activity</span>
                ) : null}
                {servicioEditandoId ? 'Guardar cambios' : 'Guardar servicio'}
              </button>

              {servicioEditandoId && (
                <button
                  type="button"
                  onClick={cancelarEdicion}
                  className="w-full text-secondary font-body-sm text-body-sm hover:text-on-surface transition-colors -mt-2"
                >
                  Cancelar edición
                </button>
              )}
            </form>
          </div>
        </aside>
      </div>

      {/* Confirmación de borrado */}
      {servicioAEliminar && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-on-background/40 backdrop-blur-sm px-4">
          <div className="bg-surface-container-lowest rounded-3xl p-8 soft-elevation max-w-sm w-full">
            <h3 className="font-title-md text-title-md text-error mb-2">Eliminar servicio</h3>
            <p className="font-body-lg text-body-lg text-secondary">
              ¿Estás seguro que querés eliminar <strong className="text-on-surface">{servicioAEliminar.nombre}</strong>?
              Esta acción no se puede deshacer.
            </p>
            <div className="flex justify-end gap-3 mt-6">
              <button
                onClick={() => setServicioAEliminar(null)}
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
