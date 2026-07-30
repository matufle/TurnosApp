// src/pages/Liquidaciones/LiquidacionesPage.tsx
import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import {
  Tabs,
  Table,
  Badge,
  Button,
  Select,
  Modal,
  NumberInput,
  TextInput,
  Textarea,
  ActionIcon,
  Group,
  Text,
  Stack,
} from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useDisclosure } from '@mantine/hooks';
import { useForm } from '@mantine/form';
import { IconRefresh, IconEye, IconPlus, IconEdit, IconWallet } from '@tabler/icons-react';
import { liquidacionesService, reglasComisionService, adelantosProfesionalService } from '../../api/liquidacionesService';
import { recursosService } from '../../api/recursosService';
import { serviciosService } from '../../api/servicioService';
import { PageSpinner } from '../../components/PageSpinner';
import { EmptyState } from '../../components/EmptyState';
import { usePermission } from '../../hooks/usePermission';
import type { Recurso } from '../../types/Recurso';
import type { Servicio } from '../../types/Servicio';
import type {
  Liquidacion,
  LiquidacionListItem,
  ReglaComision,
  AdelantoProfesional,
  TipoComision,
} from '../../types/Liquidacion';

function formatMonto(v: number) {
  return v.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' });
}

function hoyString() {
  return new Date().toISOString().slice(0, 10);
}

function mensajeError(error: unknown, fallback: string) {
  if (axios.isAxiosError(error) && error.response?.data?.detail) {
    return error.response.data.detail as string;
  }
  return fallback;
}

const ESTADO_COLOR: Record<string, string> = { Generada: 'yellow', Pagada: 'green', Anulada: 'gray' };

// Sentinel para "regla base" en el Select de servicio — el backend usa ServicioId null para eso.
const SERVICIO_BASE = '__base__';

export function LiquidacionesPage() {
  const puedeGestionar = usePermission('GestionarLiquidaciones');
  const [tabActivo, setTabActivo] = useState<string | null>('historial');

  return (
    <div className="flex flex-col gap-10 pb-12">
      <header className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface">Liquidaciones</h1>
          <p className="font-body-lg text-body-lg text-secondary mt-2">
            Comisiones de tus profesionales, generadas automáticamente según el período configurado en Configuración.
          </p>
        </div>
      </header>

      <Tabs value={tabActivo} onChange={setTabActivo} color="cyan">
        <Tabs.List>
          <Tabs.Tab value="historial">Historial</Tabs.Tab>
          <Tabs.Tab value="reglas">Reglas de comisión</Tabs.Tab>
          <Tabs.Tab value="adelantos">Adelantos</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="historial" pt="lg">
          <HistorialTab puedeGestionar={puedeGestionar} />
        </Tabs.Panel>
        <Tabs.Panel value="reglas" pt="lg">
          <ReglasComisionTab puedeGestionar={puedeGestionar} />
        </Tabs.Panel>
        <Tabs.Panel value="adelantos" pt="lg">
          <AdelantosTab puedeGestionar={puedeGestionar} />
        </Tabs.Panel>
      </Tabs>
    </div>
  );
}

// ─────────────────────────────────────────────── Historial ───────────────────────────────────────────────

function HistorialTab({ puedeGestionar }: { puedeGestionar: boolean }) {
  const [items, setItems] = useState<LiquidacionListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [generando, setGenerando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [detalle, setDetalle] = useState<Liquidacion | null>(null);
  const [cargandoDetalle, setCargandoDetalle] = useState(false);
  const [observaciones, setObservaciones] = useState('');
  const [accionando, setAccionando] = useState(false);
  const [modalOpened, { open, close }] = useDisclosure(false);

  const cargar = useCallback(async () => {
    try {
      const data = await liquidacionesService.getAll();
      setItems(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar el historial de liquidaciones.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargar();
  }, [cargar]);

  const handleGenerar = async () => {
    setGenerando(true);
    setErrorMessage(null);
    try {
      await liquidacionesService.generar();
      await cargar();
    } catch (error) {
      setErrorMessage(mensajeError(error, 'No pudimos generar las liquidaciones pendientes.'));
    } finally {
      setGenerando(false);
    }
  };

  const verDetalle = async (id: number) => {
    setObservaciones('');
    open();
    setCargandoDetalle(true);
    try {
      const data = await liquidacionesService.getById(id);
      setDetalle(data);
    } catch {
      setErrorMessage('No pudimos cargar el detalle de la liquidación.');
      close();
    } finally {
      setCargandoDetalle(false);
    }
  };

  const handleMarcarPagada = async () => {
    if (!detalle) return;
    setAccionando(true);
    try {
      const actualizada = await liquidacionesService.marcarPagada(detalle.id, { observaciones: observaciones || undefined });
      setDetalle(actualizada);
      await cargar();
    } catch (error) {
      setErrorMessage(mensajeError(error, 'No pudimos marcar la liquidación como pagada.'));
    } finally {
      setAccionando(false);
    }
  };

  const handleAnular = async () => {
    if (!detalle) return;
    setAccionando(true);
    try {
      const actualizada = await liquidacionesService.anular(detalle.id, { observaciones: observaciones || undefined });
      setDetalle(actualizada);
      await cargar();
    } catch (error) {
      setErrorMessage(mensajeError(error, 'No pudimos anular la liquidación.'));
    } finally {
      setAccionando(false);
    }
  };

  if (loading) return <PageSpinner />;

  return (
    <Stack gap="lg">
      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      {puedeGestionar && (
        <Group justify="flex-end">
          <Button leftSection={<IconRefresh size={16} />} loading={generando} onClick={handleGenerar} variant="light">
            Generar liquidaciones pendientes
          </Button>
        </Group>
      )}

      {items.length === 0 ? (
        <EmptyState
          title="Todavía no hay liquidaciones"
          description="Se generan automáticamente cuando cierra el período configurado, o podés forzar la generación ahora."
          icon={IconWallet}
          actionLabel="Generar liquidaciones pendientes"
          onAction={handleGenerar}
        />
      ) : (
        <Table.ScrollContainer minWidth={700}>
          <Table verticalSpacing="sm" highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Profesional</Table.Th>
                <Table.Th>Período</Table.Th>
                <Table.Th>Estado</Table.Th>
                <Table.Th ta="right">Bruto</Table.Th>
                <Table.Th ta="right">Adelantos</Table.Th>
                <Table.Th ta="right">Neto</Table.Th>
                <Table.Th />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {items.map((l) => (
                <Table.Tr key={l.id}>
                  <Table.Td>{l.recursoNombre}</Table.Td>
                  <Table.Td>
                    {formatFecha(l.periodoDesde)} — {formatFecha(l.periodoHasta)}
                  </Table.Td>
                  <Table.Td>
                    <Badge color={ESTADO_COLOR[l.estado] ?? 'gray'} variant="light">
                      {l.estado}
                    </Badge>
                  </Table.Td>
                  <Table.Td ta="right">${formatMonto(l.montoBrutoComision)}</Table.Td>
                  <Table.Td ta="right">${formatMonto(l.montoAdelantos)}</Table.Td>
                  <Table.Td ta="right" fw={600}>
                    ${formatMonto(l.montoNeto)}
                  </Table.Td>
                  <Table.Td>
                    <ActionIcon variant="subtle" color="gray" onClick={() => verDetalle(l.id)} aria-label="Ver detalle">
                      <IconEye size={18} />
                    </ActionIcon>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      <Modal opened={modalOpened} onClose={close} title="Detalle de liquidación" size="lg" centered>
        {cargandoDetalle || !detalle ? (
          <PageSpinner size="sm" />
        ) : (
          <Stack gap="md">
            <Group justify="space-between">
              <div>
                <Text fw={600}>{detalle.recursoNombre}</Text>
                <Text size="sm" c="dimmed">
                  {formatFecha(detalle.periodoDesde)} — {formatFecha(detalle.periodoHasta)}
                </Text>
              </div>
              <Badge color={ESTADO_COLOR[detalle.estado] ?? 'gray'} variant="light">
                {detalle.estado}
              </Badge>
            </Group>

            <Table.ScrollContainer minWidth={500}>
              <Table verticalSpacing="xs" fz="sm">
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Turno</Table.Th>
                    <Table.Th>Servicio</Table.Th>
                    <Table.Th ta="right">Precio base</Table.Th>
                    <Table.Th ta="right">Comisión</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {detalle.detalles.length === 0 ? (
                    <Table.Tr>
                      <Table.Td colSpan={4}>
                        <Text c="dimmed" size="sm" ta="center" py="sm">
                          Sin turnos en esta liquidación.
                        </Text>
                      </Table.Td>
                    </Table.Tr>
                  ) : (
                    detalle.detalles.map((d) => (
                      <Table.Tr key={d.id}>
                        <Table.Td>#{d.turnoId} — {formatFecha(d.turnoFecha)}</Table.Td>
                        <Table.Td>{d.servicioNombre}</Table.Td>
                        <Table.Td ta="right">${formatMonto(d.precioBaseAplicado)}</Table.Td>
                        <Table.Td ta="right">${formatMonto(d.montoComisionCalculado)}</Table.Td>
                      </Table.Tr>
                    ))
                  )}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>

            {detalle.adelantos.length > 0 && (
              <div>
                <Text fw={600} size="sm" mb="xs">Adelantos descontados</Text>
                <Stack gap={4}>
                  {detalle.adelantos.map((a) => (
                    <Group key={a.id} justify="space-between">
                      <Text size="sm" c="dimmed">
                        {formatFecha(a.fecha)}{a.concepto ? ` — ${a.concepto}` : ''}
                      </Text>
                      <Text size="sm">${formatMonto(a.monto)}</Text>
                    </Group>
                  ))}
                </Stack>
              </div>
            )}

            <Group justify="space-between" mt="sm">
              <Text>Bruto: ${formatMonto(detalle.montoBrutoComision)}</Text>
              <Text>Adelantos: ${formatMonto(detalle.montoAdelantos)}</Text>
              <Text fw={700}>Neto: ${formatMonto(detalle.montoNeto)}</Text>
            </Group>

            {detalle.estado === 'Pagada' && (
              <Text size="sm" c="dimmed">
                Pagada el {detalle.fechaPago ? formatFecha(detalle.fechaPago) : '—'} por {detalle.usuarioPagoNombre ?? '—'}.
              </Text>
            )}

            {puedeGestionar && detalle.estado === 'Generada' && (
              <>
                <Textarea
                  label="Observaciones (opcional)"
                  value={observaciones}
                  onChange={(e) => setObservaciones(e.currentTarget.value)}
                  autosize
                  minRows={2}
                />
                <Group grow>
                  <Button color="red" variant="light" loading={accionando} onClick={handleAnular}>
                    Anular
                  </Button>
                  <Button color="green" loading={accionando} onClick={handleMarcarPagada}>
                    Marcar pagada
                  </Button>
                </Group>
              </>
            )}
          </Stack>
        )}
      </Modal>
    </Stack>
  );
}

// ────────────────────────────────────────── Reglas de comisión ──────────────────────────────────────────

interface ReglaFormValues {
  servicio: string;
  tipo: TipoComision;
  valor: number;
}

function ReglasComisionTab({ puedeGestionar }: { puedeGestionar: boolean }) {
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [recursoId, setRecursoId] = useState<string | null>(null);
  const [reglas, setReglas] = useState<ReglaComision[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingReglas, setLoadingReglas] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [reglaEditando, setReglaEditando] = useState<ReglaComision | null>(null);
  const [modalOpened, { open, close }] = useDisclosure(false);

  const form = useForm<ReglaFormValues>({
    initialValues: { servicio: SERVICIO_BASE, tipo: 'Porcentaje', valor: 0 },
    validate: {
      valor: (value, values) =>
        value < 0 ? 'No puede ser negativo' : values.tipo === 'Porcentaje' && value > 100 ? 'No puede superar 100%' : null,
    },
  });

  useEffect(() => {
    Promise.all([recursosService.getAll(), serviciosService.getAll()])
      .then(([r, s]) => {
        setRecursos(r);
        setServicios(s);
        if (r.length > 0) setRecursoId(String(r[0].id));
        setErrorMessage(null);
      })
      .catch(() => setErrorMessage('No pudimos cargar profesionales/servicios.'))
      .finally(() => setLoading(false));
  }, []);

  const cargarReglas = useCallback(async (id: number) => {
    setLoadingReglas(true);
    try {
      const data = await reglasComisionService.getByRecurso(id);
      setReglas(data);
    } catch {
      setErrorMessage('No pudimos cargar las reglas de comisión.');
    } finally {
      setLoadingReglas(false);
    }
  }, []);

  useEffect(() => {
    if (recursoId) cargarReglas(Number(recursoId));
  }, [recursoId, cargarReglas]);

  const abrirCrear = () => {
    form.reset();
    setReglaEditando(null);
    open();
  };

  const abrirEditar = (regla: ReglaComision) => {
    form.setValues({
      servicio: regla.servicioId === null ? SERVICIO_BASE : String(regla.servicioId),
      tipo: regla.tipo,
      valor: regla.valor,
    });
    setReglaEditando(regla);
    open();
  };

  const handleSubmit = async (values: ReglaFormValues) => {
    if (!recursoId) return;
    setSubmitting(true);
    try {
      if (reglaEditando) {
        await reglasComisionService.update(reglaEditando.id, { tipo: values.tipo, valor: values.valor, activo: true });
      } else {
        await reglasComisionService.create({
          recursoId: Number(recursoId),
          servicioId: values.servicio === SERVICIO_BASE ? null : Number(values.servicio),
          tipo: values.tipo,
          valor: values.valor,
        });
      }
      close();
      await cargarReglas(Number(recursoId));
    } catch (error) {
      setErrorMessage(mensajeError(error, 'No pudimos guardar la regla de comisión.'));
    } finally {
      setSubmitting(false);
    }
  };

  const handleToggleActivo = async (regla: ReglaComision) => {
    try {
      await reglasComisionService.update(regla.id, { tipo: regla.tipo, valor: regla.valor, activo: !regla.activo });
      if (recursoId) await cargarReglas(Number(recursoId));
    } catch (error) {
      setErrorMessage(mensajeError(error, 'No pudimos actualizar la regla.'));
    }
  };

  if (loading) return <PageSpinner />;

  const serviciosDisponibles = servicios.filter((s) => !reglas.some((r) => r.activo && r.servicioId === s.id));

  return (
    <Stack gap="lg">
      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      <Group justify="space-between" wrap="wrap">
        <Select
          label="Profesional"
          data={recursos.map((r) => ({ value: String(r.id), label: r.nombre }))}
          value={recursoId}
          onChange={setRecursoId}
          w={280}
          allowDeselect={false}
        />
        {puedeGestionar && recursoId && (
          <Button leftSection={<IconPlus size={16} />} onClick={abrirCrear} mt="auto">
            Nueva regla
          </Button>
        )}
      </Group>

      {loadingReglas ? (
        <PageSpinner size="sm" />
      ) : reglas.length === 0 ? (
        <Text c="dimmed" size="sm">Este profesional todavía no tiene reglas de comisión configuradas.</Text>
      ) : (
        <Table.ScrollContainer minWidth={500}>
          <Table verticalSpacing="sm" highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Servicio</Table.Th>
                <Table.Th>Tipo</Table.Th>
                <Table.Th ta="right">Valor</Table.Th>
                <Table.Th>Estado</Table.Th>
                <Table.Th />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {reglas.map((r) => (
                <Table.Tr key={r.id}>
                  <Table.Td>{r.servicioId === null ? 'Todos los servicios (base)' : r.servicioNombre}</Table.Td>
                  <Table.Td>{r.tipo === 'Porcentaje' ? 'Porcentaje' : 'Monto fijo'}</Table.Td>
                  <Table.Td ta="right">{r.tipo === 'Porcentaje' ? `${r.valor.toFixed(2)}%` : `$${formatMonto(r.valor)}`}</Table.Td>
                  <Table.Td>
                    <Badge color={r.activo ? 'green' : 'gray'} variant="light">
                      {r.activo ? 'Activa' : 'Inactiva'}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    {puedeGestionar && (
                      <Group gap={4}>
                        <ActionIcon variant="subtle" color="gray" onClick={() => abrirEditar(r)} aria-label="Editar">
                          <IconEdit size={16} />
                        </ActionIcon>
                        <Button size="xs" variant="subtle" color={r.activo ? 'red' : 'green'} onClick={() => handleToggleActivo(r)}>
                          {r.activo ? 'Desactivar' : 'Activar'}
                        </Button>
                      </Group>
                    )}
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      <Modal opened={modalOpened} onClose={close} title={reglaEditando ? 'Editar regla de comisión' : 'Nueva regla de comisión'} centered>
        <form onSubmit={form.onSubmit(handleSubmit)} className="flex flex-col gap-4">
          <Select
            label="Servicio"
            description="Dejá 'Todos los servicios' para la regla base del profesional"
            data={[
              { value: SERVICIO_BASE, label: 'Todos los servicios (base)' },
              ...(reglaEditando ? servicios : serviciosDisponibles).map((s) => ({ value: String(s.id), label: s.nombre })),
            ]}
            disabled={!!reglaEditando}
            allowDeselect={false}
            {...form.getInputProps('servicio')}
          />

          <Select
            label="Tipo de comisión"
            data={[
              { value: 'Porcentaje', label: 'Porcentaje del precio del servicio' },
              { value: 'MontoFijo', label: 'Monto fijo por turno' },
            ]}
            allowDeselect={false}
            {...form.getInputProps('tipo')}
          />

          <NumberInput
            label="Valor"
            min={0}
            max={form.values.tipo === 'Porcentaje' ? 100 : undefined}
            decimalScale={2}
            suffix={form.values.tipo === 'Porcentaje' ? '%' : undefined}
            prefix={form.values.tipo === 'MontoFijo' ? '$' : undefined}
            {...form.getInputProps('valor')}
          />

          <Button type="submit" loading={submitting} fullWidth mt="sm">
            {reglaEditando ? 'Guardar cambios' : 'Crear regla'}
          </Button>
        </form>
      </Modal>
    </Stack>
  );
}

// ────────────────────────────────────────────────── Adelantos ──────────────────────────────────────────────────

interface AdelantoFormValues {
  monto: number;
  fecha: string;
  concepto: string;
}

function AdelantosTab({ puedeGestionar }: { puedeGestionar: boolean }) {
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [recursoId, setRecursoId] = useState<string | null>(null);
  const [adelantos, setAdelantos] = useState<AdelantoProfesional[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingAdelantos, setLoadingAdelantos] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [modalOpened, { open, close }] = useDisclosure(false);

  const form = useForm<AdelantoFormValues>({
    initialValues: { monto: 0, fecha: hoyString(), concepto: '' },
    validate: {
      monto: (value) => (value > 0 ? null : 'Tiene que ser mayor a cero'),
    },
  });

  useEffect(() => {
    recursosService
      .getAll()
      .then((r) => {
        setRecursos(r);
        if (r.length > 0) setRecursoId(String(r[0].id));
        setErrorMessage(null);
      })
      .catch(() => setErrorMessage('No pudimos cargar los profesionales.'))
      .finally(() => setLoading(false));
  }, []);

  const cargarAdelantos = useCallback(async (id: number) => {
    setLoadingAdelantos(true);
    try {
      const data = await adelantosProfesionalService.getByRecurso(id);
      setAdelantos(data);
    } catch {
      setErrorMessage('No pudimos cargar los adelantos.');
    } finally {
      setLoadingAdelantos(false);
    }
  }, []);

  useEffect(() => {
    if (recursoId) cargarAdelantos(Number(recursoId));
  }, [recursoId, cargarAdelantos]);

  const abrirCrear = () => {
    form.reset();
    form.setFieldValue('fecha', hoyString());
    open();
  };

  const handleSubmit = async (values: AdelantoFormValues) => {
    if (!recursoId) return;
    setSubmitting(true);
    try {
      await adelantosProfesionalService.create({
        recursoId: Number(recursoId),
        monto: values.monto,
        fecha: values.fecha,
        concepto: values.concepto || undefined,
      });
      close();
      await cargarAdelantos(Number(recursoId));
    } catch (error) {
      setErrorMessage(mensajeError(error, 'No pudimos cargar el adelanto.'));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <PageSpinner />;

  return (
    <Stack gap="lg">
      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      <Group justify="space-between" wrap="wrap">
        <Select
          label="Profesional"
          data={recursos.map((r) => ({ value: String(r.id), label: r.nombre }))}
          value={recursoId}
          onChange={setRecursoId}
          w={280}
          allowDeselect={false}
        />
        {puedeGestionar && recursoId && (
          <Button leftSection={<IconPlus size={16} />} onClick={abrirCrear} mt="auto">
            Nuevo adelanto
          </Button>
        )}
      </Group>

      {loadingAdelantos ? (
        <PageSpinner size="sm" />
      ) : adelantos.length === 0 ? (
        <Text c="dimmed" size="sm">Este profesional todavía no tiene adelantos cargados.</Text>
      ) : (
        <Table.ScrollContainer minWidth={500}>
          <Table verticalSpacing="sm" highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Fecha</Table.Th>
                <Table.Th>Concepto</Table.Th>
                <Table.Th ta="right">Monto</Table.Th>
                <Table.Th>Asignado</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {adelantos.map((a) => (
                <Table.Tr key={a.id}>
                  <Table.Td>{formatFecha(a.fecha)}</Table.Td>
                  <Table.Td>{a.concepto ?? '—'}</Table.Td>
                  <Table.Td ta="right">${formatMonto(a.monto)}</Table.Td>
                  <Table.Td>
                    <Badge color={a.liquidacionId ? 'green' : 'yellow'} variant="light">
                      {a.liquidacionId ? 'Sí' : 'Pendiente'}
                    </Badge>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      <Modal opened={modalOpened} onClose={close} title="Nuevo adelanto" centered>
        <form onSubmit={form.onSubmit(handleSubmit)} className="flex flex-col gap-4">
          <DatePickerInput label="Fecha" {...form.getInputProps('fecha')} maxDate={hoyString()} />

          <NumberInput label="Monto" min={0} decimalScale={2} prefix="$" {...form.getInputProps('monto')} />

          <TextInput label="Concepto (opcional)" placeholder="Ej: retiro a cuenta" {...form.getInputProps('concepto')} />

          <Button type="submit" loading={submitting} fullWidth mt="sm">
            Cargar adelanto
          </Button>
        </form>
      </Modal>
    </Stack>
  );
}
