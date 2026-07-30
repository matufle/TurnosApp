// src/pages/ListaEspera/ListaEsperaPage.tsx
import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import { Modal, Select, Button, Menu, ActionIcon, Badge } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconDots, IconBan, IconHourglassHigh } from '@tabler/icons-react';
import { listaEsperaService } from '../../api/listaEsperaService';
import { clientesService } from '../../api/clientesService';
import { recursosService } from '../../api/recursosService';
import { serviciosService } from '../../api/servicioService';
import { EmptyState } from '../../components/EmptyState';
import { PageSpinner } from '../../components/PageSpinner';
import { RequirePermission } from '../../auth/RequirePermission';
import type { ListaEsperaEntry } from '../../types/ListaEspera';
import type { Cliente } from '../../types/Cliente';
import type { Recurso } from '../../types/Recurso';
import type { Servicio } from '../../types/Servicio';

// Mismo patrón que useMetricasFiltro/HistorialCobrosPage: DatePickerInput entrega strings
// "YYYY-MM-DD"; se mantienen como string y solo se les agrega la hora al enviar al backend,
// para no disparar el bug de `new Date("YYYY-MM-DD")` interpretado como UTC.
function inicioDeDia(fecha: string): string {
  return `${fecha}T00:00:00`;
}
function finDeDia(fecha: string): string {
  return `${fecha}T23:59:59.999`;
}

interface ListaEsperaFormValues {
  clienteId: string;
  recursoId: string;
  servicioId: string | null;
  fechaDesde: string | null;
  fechaHasta: string | null;
}

const ESTADO_COLOR: Record<ListaEsperaEntry['estado'], string> = {
  Activa: 'blue',
  Notificada: 'green',
  Cancelada: 'gray',
};

export function ListaEsperaPage() {
  const [entradas, setEntradas] = useState<ListaEsperaEntry[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [modalOpened, { open, close }] = useDisclosure(false);

  const form = useForm<ListaEsperaFormValues>({
    initialValues: { clienteId: '', recursoId: '', servicioId: null, fechaDesde: null, fechaHasta: null },
    validate: {
      clienteId: isNotEmpty('Elegí un cliente'),
      recursoId: isNotEmpty('Elegí un recurso'),
      fechaDesde: isNotEmpty('Elegí la fecha desde'),
      fechaHasta: (value, values) => {
        if (!value) return 'Elegí la fecha hasta';
        if (values.fechaDesde && value < values.fechaDesde) return 'Debe ser posterior a la fecha desde';
        return null;
      },
    },
  });

  const cargarTodo = useCallback(async () => {
    try {
      const [entradasData, clientesData, recursosData, serviciosData] = await Promise.all([
        listaEsperaService.getAll(),
        clientesService.getAll(),
        recursosService.getAll(),
        serviciosService.getAll(),
      ]);
      setEntradas(entradasData);
      setClientes(clientesData);
      setRecursos(recursosData);
      setServicios(serviciosData);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar la lista de espera.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarTodo();
  }, [cargarTodo]);

  const abrirCrear = () => {
    form.reset();
    open();
  };

  const handleSubmit = async (values: ListaEsperaFormValues) => {
    setSubmitting(true);
    try {
      const creada = await listaEsperaService.crear({
        clienteId: Number(values.clienteId),
        recursoId: Number(values.recursoId),
        servicioId: values.servicioId ? Number(values.servicioId) : null,
        fechaDesde: inicioDeDia(values.fechaDesde!),
        fechaHasta: finDeDia(values.fechaHasta!),
      });
      setEntradas((prev) => [creada, ...prev]);
      close();
      form.reset();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos anotar al cliente en la lista de espera.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancelar = async (entrada: ListaEsperaEntry) => {
    try {
      const actualizada = await listaEsperaService.cancelar(entrada.id);
      setEntradas((prev) => prev.map((e) => (e.id === actualizada.id ? actualizada : e)));
    } catch {
      setErrorMessage('No pudimos cancelar esta entrada.');
    }
  };

  if (loading) {
    return (
      <PageSpinner />
    );
  }

  return (
    <div className="flex flex-col gap-10 pb-12">
      <header className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface">Lista de Espera</h1>
          <p className="font-body-lg text-body-lg text-secondary mt-2">
            Anotá clientes para avisarles automáticamente cuando se libere un turno.
          </p>
        </div>
        <RequirePermission permiso="GestionarListaEspera">
          <button
            onClick={abrirCrear}
            className="bg-primary text-on-primary font-label-md text-label-md px-6 py-3 rounded-full soft-elevation hover:bg-primary-container hover:shadow-lg transition-all duration-200 flex items-center gap-2 whitespace-nowrap w-full md:w-auto justify-center"
          >
            <span className="material-symbols-outlined">add</span> Anotar cliente
          </button>
        </RequirePermission>
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

      {entradas.length === 0 ? (
        <EmptyState
          title="Sin entradas en lista de espera"
          description="Cuando un turno se cancele, el sistema avisa por email a los clientes anotados acá cuyo horario coincida."
          icon={IconHourglassHigh}
          actionLabel="Anotar cliente"
          onAction={abrirCrear}
        />
      ) : (
        <div className="overflow-x-auto bg-surface-container-lowest rounded-3xl soft-elevation">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-surface-container-low border-b border-outline-variant">
                <th className="py-4 px-6 font-label-md text-label-md text-secondary">Cliente</th>
                <th className="py-4 px-6 font-label-md text-label-md text-secondary">Recurso</th>
                <th className="py-4 px-6 font-label-md text-label-md text-secondary">Servicio</th>
                <th className="py-4 px-6 font-label-md text-label-md text-secondary">Rango deseado</th>
                <th className="py-4 px-6 font-label-md text-label-md text-secondary">Estado</th>
                <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-surface-variant">
              {entradas.map((entrada) => (
                <tr key={entrada.id} className="hover:bg-surface-container transition-colors">
                  <td className="py-4 px-6 font-title-md text-body-sm text-on-surface">{entrada.clienteNombreCompleto}</td>
                  <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">{entrada.recursoNombre}</td>
                  <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">
                    {entrada.servicioNombre ?? 'Cualquiera'}
                  </td>
                  <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">
                    {new Date(entrada.fechaDesde).toLocaleDateString('es-AR', { timeZone: 'UTC' })} –{' '}
                    {new Date(entrada.fechaHasta).toLocaleDateString('es-AR', { timeZone: 'UTC' })}
                  </td>
                  <td className="py-4 px-6">
                    <Badge color={ESTADO_COLOR[entrada.estado]} variant="light">
                      {entrada.estado}
                    </Badge>
                  </td>
                  <td className="py-4 px-6 text-right">
                    {entrada.estado !== 'Cancelada' && (
                      <RequirePermission permiso="GestionarListaEspera">
                        <Menu shadow="md" position="bottom-end" withinPortal>
                          <Menu.Target>
                            <ActionIcon variant="subtle" color="gray" aria-label="Más opciones">
                              <IconDots size={18} />
                            </ActionIcon>
                          </Menu.Target>
                          <Menu.Dropdown>
                            <Menu.Item
                              leftSection={<IconBan size={16} />}
                              color="red"
                              onClick={() => handleCancelar(entrada)}
                            >
                              Cancelar
                            </Menu.Item>
                          </Menu.Dropdown>
                        </Menu>
                      </RequirePermission>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Modal opened={modalOpened} onClose={close} title="Anotar cliente en lista de espera" centered>
        <form onSubmit={form.onSubmit(handleSubmit)} className="flex flex-col gap-4">
          <Select
            label="Cliente"
            placeholder="Elegí un cliente"
            data={clientes.map((c) => ({ value: c.id.toString(), label: `${c.nombre} ${c.apellido}` }))}
            searchable
            required
            {...form.getInputProps('clienteId')}
          />

          <Select
            label="Recurso"
            placeholder="Elegí un recurso"
            data={recursos.map((r) => ({ value: r.id.toString(), label: r.nombre }))}
            required
            {...form.getInputProps('recursoId')}
          />

          <Select
            label="Servicio (opcional)"
            description="Si no elegís uno, cualquier turno liberado en el recurso cuenta como coincidencia"
            placeholder="Cualquiera"
            data={servicios.map((s) => ({ value: s.id.toString(), label: s.nombre }))}
            clearable
            {...form.getInputProps('servicioId')}
          />

          <DatePickerInput label="Desde" placeholder="Fecha desde" required {...form.getInputProps('fechaDesde')} />
          <DatePickerInput label="Hasta" placeholder="Fecha hasta" required {...form.getInputProps('fechaHasta')} />

          <Button type="submit" loading={submitting} fullWidth mt="sm">
            Anotar en lista de espera
          </Button>
        </form>
      </Modal>
    </div>
  );
}
