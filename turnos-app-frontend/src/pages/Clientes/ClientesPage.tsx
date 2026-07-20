import { useEffect, useMemo, useState } from 'react';
import axios from 'axios';
import { useForm, isNotEmpty } from '@mantine/form';
import { clientesService } from '../../api/clientesService';
import { turnosService } from '../../api/turnosService';
import type { Cliente } from '../../types/Cliente';
import type { Turno } from '../../types/Turno';

function iniciales(nombre: string, apellido: string) {
  return `${nombre[0] ?? ''}${apellido[0] ?? ''}`.toUpperCase();
}

export function ClientesPage() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [searchTerm, setSearchTerm] = useState('');
  const [modalAbierto, setModalAbierto] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [clienteEditandoId, setClienteEditandoId] = useState<number | null>(null);

  const [clienteAEliminar, setClienteAEliminar] = useState<Cliente | null>(null);
  const [eliminando, setEliminando] = useState(false);

  const form = useForm({
    initialValues: { nombre: '', apellido: '', email: '', telefono: '', notasAdicionales: '' },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      apellido: isNotEmpty('El apellido es obligatorio'),
    },
  });

  const cargar = async () => {
    setLoading(true);
    try {
      const [clientesData, turnosData] = await Promise.all([clientesService.getAll(), turnosService.getAll()]);
      setClientes(clientesData);
      setTurnos(turnosData);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los clientes.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    cargar();
  }, []);

  const clientesFiltrados = useMemo(() => {
    const termino = searchTerm.toLowerCase();
    return clientes.filter(
      (c) =>
        c.nombre.toLowerCase().includes(termino) ||
        c.apellido.toLowerCase().includes(termino) ||
        c.email?.toLowerCase().includes(termino)
    );
  }, [clientes, searchTerm]);

  const resumenPorCliente = useMemo(() => {
    const ahora = new Date();
    const mapa = new Map<
      number,
      { total: number; proximo: Turno | null; ultimo: Turno | null }
    >();

    clientes.forEach((c) => {
      const turnosCliente = turnos.filter((t) => t.clienteId === c.id && t.estado !== 'Cancelado');
      const futuros = turnosCliente
        .filter((t) => new Date(t.fechaHoraInicio) >= ahora)
        .sort((a, b) => new Date(a.fechaHoraInicio).getTime() - new Date(b.fechaHoraInicio).getTime());
      const pasados = turnosCliente
        .filter((t) => new Date(t.fechaHoraInicio) < ahora)
        .sort((a, b) => new Date(b.fechaHoraInicio).getTime() - new Date(a.fechaHoraInicio).getTime());

      mapa.set(c.id, {
        total: turnosCliente.length,
        proximo: futuros[0] ?? null,
        ultimo: pasados[0] ?? null,
      });
    });

    return mapa;
  }, [clientes, turnos]);

  const abrirNuevo = () => {
    form.reset();
    setClienteEditandoId(null);
    setModalAbierto(true);
  };

  const abrirEdicion = (c: Cliente) => {
    form.setValues({
      nombre: c.nombre,
      apellido: c.apellido,
      email: c.email ?? '',
      telefono: c.telefono ?? '',
      notasAdicionales: c.notasAdicionales ?? '',
    });
    setClienteEditandoId(c.id);
    setModalAbierto(true);
  };

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    try {
      if (clienteEditandoId) {
        await clientesService.update(clienteEditandoId, values);
      } else {
        await clientesService.create(values);
      }
      setModalAbierto(false);
      form.reset();
      setClienteEditandoId(null);
      await cargar();
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
    if (!clienteAEliminar) return;
    setEliminando(true);
    try {
      await clientesService.delete(clienteAEliminar.id);
      setClientes((prev) => prev.filter((c) => c.id !== clienteAEliminar.id));
      setClienteAEliminar(null);
    } catch {
      setErrorMessage('No pudimos eliminar el cliente. Puede que tenga turnos asociados.');
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
    <div className="flex flex-col gap-8 pb-12">
      {/* Encabezado */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-2">
            Clientes
          </h1>
          <p className="font-body-lg text-body-lg text-secondary">
            Gestioná tu lista de clientes y sus próximos turnos.
          </p>
        </div>
        <div className="flex items-center gap-4 w-full md:w-auto">
          <div className="relative grow md:w-64">
            <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline">
              search
            </span>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Buscar cliente..."
              className="w-full bg-surface-container-lowest border border-outline-variant text-on-surface rounded-full py-2.5 pl-10 pr-4 font-body-sm text-body-sm focus:outline-none focus:border-primary-container focus:ring-1 focus:ring-primary-container transition-all"
            />
          </div>
          <button
            onClick={abrirNuevo}
            className="bg-primary text-on-primary font-label-md text-label-md px-6 py-2.5 rounded-full hover:bg-primary-container hover:text-on-primary-container transition-colors shadow-sm whitespace-nowrap flex items-center gap-2"
          >
            <span className="material-symbols-outlined text-[18px]">add</span>
            Nuevo cliente
          </button>
        </div>
      </div>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      {clientes.length === 0 ? (
        <div className="bg-surface-container-lowest rounded-3xl p-12 soft-elevation flex flex-col items-center text-center gap-3">
          <span className="material-symbols-outlined text-4xl text-outline">group</span>
          <h3 className="font-title-md text-title-md text-on-surface">Sin clientes registrados</h3>
          <p className="font-body-sm text-body-sm text-secondary max-w-sm">
            Agregá tu primer cliente para empezar a asignarle turnos.
          </p>
          <button
            onClick={abrirNuevo}
            className="mt-2 bg-primary-container text-on-primary-container font-title-md text-title-md px-6 py-3 rounded-full hover:bg-primary transition-colors"
          >
            Crear mi primer cliente
          </button>
        </div>
      ) : clientesFiltrados.length === 0 ? (
        <p className="font-body-lg text-body-lg text-secondary text-center py-8">
          No se encontraron resultados para tu búsqueda.
        </p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {clientesFiltrados.map((c) => {
            const resumen = resumenPorCliente.get(c.id);
            const proximo = resumen?.proximo;
            const ultimo = resumen?.ultimo;

            return (
              <div
                key={c.id}
                className="bg-surface-container-lowest rounded-[24px] p-6 soft-elevation flex flex-col justify-between border border-outline-variant/30 hover:shadow-[0px_8px_30px_rgba(0,188,212,0.12)] transition-shadow relative overflow-hidden group"
              >
                <div className="absolute top-0 left-0 w-2 h-full bg-primary-container -translate-x-full group-hover:translate-x-0 transition-transform duration-300" />

                <div className="flex justify-between items-start mb-6 gap-2">
                  <div className="flex items-center gap-4 min-w-0">
                    <div className="w-12 h-12 rounded-full bg-surface-container-high flex items-center justify-center text-primary font-title-md text-title-md shrink-0">
                      {iniciales(c.nombre, c.apellido)}
                    </div>
                    <div className="min-w-0">
                      <h3 className="font-title-md text-title-md text-on-surface truncate">
                        {c.nombre} {c.apellido}
                      </h3>
                      <p className="font-body-sm text-body-sm text-secondary truncate">
                        {c.email || c.telefono || 'Sin datos de contacto'}
                      </p>
                    </div>
                  </div>

                  <div className="flex gap-1 opacity-100 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity shrink-0">
                    <button
                      aria-label="Editar"
                      onClick={() => abrirEdicion(c)}
                      className="p-1.5 rounded-lg text-secondary hover:bg-surface-container hover:text-primary transition-colors"
                    >
                      <span className="material-symbols-outlined text-[18px]">edit</span>
                    </button>
                    <button
                      aria-label="Eliminar"
                      onClick={() => setClienteAEliminar(c)}
                      className="p-1.5 rounded-lg text-secondary hover:bg-error-container hover:text-error transition-colors"
                    >
                      <span className="material-symbols-outlined text-[18px]">delete</span>
                    </button>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4 pt-4 border-t border-outline-variant/30">
                  <div>
                    <p className="font-label-md text-label-md text-secondary uppercase">
                      {proximo ? 'Próximo turno' : 'Último turno'}
                    </p>
                    <p
                      className={`font-body-sm text-body-sm mt-1 ${
                        proximo ? 'text-primary font-semibold' : 'text-on-surface'
                      }`}
                    >
                      {proximo
                        ? new Date(proximo.fechaHoraInicio).toLocaleString('es-AR', {
                            day: '2-digit',
                            month: 'short',
                            hour: '2-digit',
                            minute: '2-digit',
                          })
                        : ultimo
                          ? new Date(ultimo.fechaHoraInicio).toLocaleString('es-AR', {
                              day: '2-digit',
                              month: 'short',
                              hour: '2-digit',
                              minute: '2-digit',
                            })
                          : 'Sin turnos'}
                    </p>
                  </div>
                  <div>
                    <p className="font-label-md text-label-md text-secondary uppercase">Total turnos</p>
                    <p className="font-body-sm text-body-sm text-on-surface mt-1">
                      {resumen?.total ?? 0} {resumen?.total === 1 ? 'visita' : 'visitas'}
                    </p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Modal de alta / edición */}
      {modalAbierto && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-on-background/40 backdrop-blur-sm px-4">
          <div className="bg-surface-container-lowest rounded-3xl p-8 soft-elevation max-w-md w-full max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-6">
              <h2 className="font-title-md text-title-md text-on-surface">
                {clienteEditandoId ? 'Editar cliente' : 'Nuevo cliente'}
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
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block font-label-md text-label-md text-secondary mb-1">Nombre</label>
                  <input
                    type="text"
                    placeholder="Laura"
                    className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                    {...form.getInputProps('nombre')}
                  />
                  {form.errors.nombre && (
                    <p className="font-body-sm text-body-sm text-error mt-1">{form.errors.nombre}</p>
                  )}
                </div>
                <div>
                  <label className="block font-label-md text-label-md text-secondary mb-1">Apellido</label>
                  <input
                    type="text"
                    placeholder="Gómez"
                    className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                    {...form.getInputProps('apellido')}
                  />
                  {form.errors.apellido && (
                    <p className="font-body-sm text-body-sm text-error mt-1">{form.errors.apellido}</p>
                  )}
                </div>
              </div>

              <div>
                <label className="block font-label-md text-label-md text-secondary mb-1">Teléfono</label>
                <input
                  type="text"
                  placeholder="Ej: 1122334455"
                  className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                  {...form.getInputProps('telefono')}
                />
              </div>

              <div>
                <label className="block font-label-md text-label-md text-secondary mb-1">Email</label>
                <input
                  type="email"
                  placeholder="cliente@email.com"
                  className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all"
                  {...form.getInputProps('email')}
                />
              </div>

              <div>
                <label className="block font-label-md text-label-md text-secondary mb-1">Notas (opcional)</label>
                <textarea
                  rows={3}
                  placeholder="Preferencias, alergias, observaciones..."
                  className="w-full bg-surface-bright border border-outline-variant rounded-xl px-4 py-3 font-body-lg text-on-surface placeholder:text-outline/50 focus:outline-none focus:border-primary-container focus:ring-2 focus:ring-primary-container/20 transition-all resize-none"
                  {...form.getInputProps('notasAdicionales')}
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
                Guardar
              </button>
            </form>
          </div>
        </div>
      )}

      {/* Confirmación de borrado */}
      {clienteAEliminar && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-on-background/40 backdrop-blur-sm px-4">
          <div className="bg-surface-container-lowest rounded-3xl p-8 soft-elevation max-w-sm w-full">
            <h3 className="font-title-md text-title-md text-error mb-2">Eliminar cliente</h3>
            <p className="font-body-lg text-body-lg text-secondary">
              ¿Estás seguro que querés eliminar a{' '}
              <strong className="text-on-surface">
                {clienteAEliminar.nombre} {clienteAEliminar.apellido}
              </strong>
              ? Esta acción no se puede deshacer.
            </p>
            <div className="flex justify-end gap-3 mt-6">
              <button
                onClick={() => setClienteAEliminar(null)}
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
