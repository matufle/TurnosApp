// src/pages/Turnos/RegistrarCobroModal.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import { Modal, Select, NumberInput, Button, Stack, Group, Text, Divider, ActionIcon, Badge } from '@mantine/core';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconEdit, IconX } from '@tabler/icons-react';
import { metodosPagoService } from '../../api/metodosPagoService';
import { cobrosService } from '../../api/cobrosService';
import { usePermission } from '../../hooks/usePermission';
import type { Turno } from '../../types/Turno';
import type { MetodoPago } from '../../types/MetodoPago';
import type { Cobro } from '../../types/Cobro';

interface RegistrarCobroModalProps {
  opened: boolean;
  onClose: () => void;
  turno: Turno;
  onCobroGuardado: () => void | Promise<void>;
}

interface CobroFormValues {
  metodoPagoId: string;
  precioBase: number;
}

function formatMonto(valor: number) {
  return valor.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function calcularPreview(precioBase: number, metodo: MetodoPago) {
  const signo = metodo.tipoModificador === 'Bonificacion' ? -1 : metodo.tipoModificador === 'Recargo' ? 1 : 0;
  const montoModificador = (signo * (precioBase * metodo.porcentajeModificador)) / 100;
  const precioFinal = precioBase + montoModificador;
  const montoComision = (precioFinal * metodo.porcentajeComision) / 100;
  const gananciaNeta = precioFinal - montoComision;
  return { montoModificador, precioFinal, montoComision, gananciaNeta };
}

export function RegistrarCobroModal({ opened, onClose, turno, onCobroGuardado }: RegistrarCobroModalProps) {
  const puedeVerGananciaNeta = usePermission('VerGananciaNeta');
  const [metodosPago, setMetodosPago] = useState<MetodoPago[]>([]);
  const [cobros, setCobros] = useState<Cobro[]>([]);
  const [cargandoDatos, setCargandoDatos] = useState(true);
  const [cobroEditandoId, setCobroEditandoId] = useState<number | null>(null);
  const [guardando, setGuardando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm<CobroFormValues>({
    initialValues: { metodoPagoId: '', precioBase: 0 },
    validate: {
      metodoPagoId: isNotEmpty('Elegí un método de pago'),
      precioBase: (value) => (value > 0 ? null : 'El monto debe ser mayor a cero'),
    },
  });

  useEffect(() => {
    if (!opened) return;
    let activo = true;

    async function cargar() {
      setCargandoDatos(true);
      try {
        const [metodos, cobrosDelTurno] = await Promise.all([
          metodosPagoService.getAll(),
          cobrosService.getByTurno(turno.id),
        ]);
        if (!activo) return;
        setMetodosPago(metodos.filter((m) => m.activo));
        setCobros(cobrosDelTurno);
        setErrorMessage(null);
        form.setFieldValue('precioBase', Number(turno.saldoPendiente.toFixed(2)));
      } catch {
        if (activo) setErrorMessage('No pudimos cargar los métodos de pago o los cobros del turno.');
      } finally {
        if (activo) setCargandoDatos(false);
      }
    }

    cargar();
    return () => {
      activo = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opened, turno.id]);

  const cobroEditando = cobros.find((c) => c.id === cobroEditandoId) ?? null;
  const saldoDisponible = turno.saldoPendiente + (cobroEditando?.precioBase ?? 0);
  const turnoCobrable = turno.estado !== 'Cancelado' && turno.estado !== 'Ausente';

  const metodoSeleccionado = metodosPago.find((m) => String(m.id) === form.values.metodoPagoId);
  const preview = metodoSeleccionado ? calcularPreview(form.values.precioBase, metodoSeleccionado) : null;

  const abrirNuevo = () => {
    form.setValues({ metodoPagoId: '', precioBase: Number(saldoDisponible.toFixed(2)) });
    setCobroEditandoId(null);
  };

  const abrirEditar = (cobro: Cobro) => {
    form.setValues({
      metodoPagoId: cobro.metodoPagoId ? String(cobro.metodoPagoId) : '',
      precioBase: cobro.precioBase,
    });
    setCobroEditandoId(cobro.id);
  };

  const handleSubmit = async (values: CobroFormValues) => {
    setGuardando(true);
    setErrorMessage(null);
    try {
      if (cobroEditandoId) {
        await cobrosService.actualizar(cobroEditandoId, {
          metodoPagoId: Number(values.metodoPagoId),
          precioBase: values.precioBase,
        });
      } else {
        await cobrosService.crear({
          turnoId: turno.id,
          metodoPagoId: Number(values.metodoPagoId),
          precioBase: values.precioBase,
        });
      }

      const cobrosActualizados = await cobrosService.getByTurno(turno.id);
      setCobros(cobrosActualizados);
      form.setValues({ metodoPagoId: '', precioBase: 0 });
      setCobroEditandoId(null);
      await onCobroGuardado();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos guardar el cobro.');
      }
    } finally {
      setGuardando(false);
    }
  };

  return (
    <Modal opened={opened} onClose={onClose} title="Cobros del turno" centered size="md">
      <Stack gap="md">
        {cargandoDatos ? (
          <div className="flex items-center justify-center py-8">
            <span className="material-symbols-outlined animate-spin text-primary text-3xl">progress_activity</span>
          </div>
        ) : (
          <>
            {cobros.length > 0 && (
              <Stack gap="xs">
                {cobros.map((cobro) => (
                  <Group key={cobro.id} justify="space-between" wrap="nowrap" className="border border-outline-variant/40 rounded-xl px-3 py-2">
                    <div>
                      <Text size="sm" fw={500}>
                        {cobro.nombreMetodoPagoSnapshot} — ${formatMonto(cobro.precioFinal)}
                      </Text>
                      <Text size="xs" c="dimmed">
                        Base ${formatMonto(cobro.precioBase)}
                        {puedeVerGananciaNeta && cobro.gananciaNeta !== null
                          ? ` · Ganancia ${formatMonto(cobro.gananciaNeta)}`
                          : ''}
                      </Text>
                    </div>
                    <ActionIcon variant="subtle" aria-label="Editar cobro" onClick={() => abrirEditar(cobro)}>
                      <IconEdit size={16} />
                    </ActionIcon>
                  </Group>
                ))}
              </Stack>
            )}

            {!turnoCobrable ? (
              <Text size="sm" c="dimmed">
                Este turno está en estado "{turno.estado}" y no admite cobros nuevos.
              </Text>
            ) : (
              <>
                <Divider label={cobroEditandoId ? 'Editar cobro' : 'Registrar nuevo cobro'} labelPosition="left" />

                {saldoDisponible <= 0 && !cobroEditandoId ? (
                  <Group gap="xs">
                    <Badge color="green" variant="light">
                      Pagado
                    </Badge>
                    <Text size="sm" c="dimmed">
                      Este turno ya no tiene saldo pendiente.
                    </Text>
                  </Group>
                ) : (
                  <form onSubmit={form.onSubmit(handleSubmit)}>
                    <Stack gap="sm">
                      <Select
                        label="Método de pago"
                        placeholder="Elegí un método"
                        data={metodosPago.map((m) => ({
                          value: String(m.id),
                          label: `${m.nombre} (${m.tipoModificador === 'Recargo' ? '+' : m.tipoModificador === 'Bonificacion' ? '-' : ''}${m.porcentajeModificador}%)`,
                        }))}
                        {...form.getInputProps('metodoPagoId')}
                      />

                      <NumberInput
                        label="Monto a cobrar"
                        description={`Saldo disponible: $${formatMonto(saldoDisponible)}`}
                        min={0.01}
                        max={saldoDisponible}
                        decimalScale={2}
                        prefix="$"
                        {...form.getInputProps('precioBase')}
                      />

                      {preview && (
                        <div className="bg-surface-container-low rounded-xl p-3 flex flex-col gap-1">
                          <Group justify="space-between">
                            <Text size="xs" c="dimmed">
                              Modificador
                            </Text>
                            <Text size="xs">
                              {preview.montoModificador === 0
                                ? '—'
                                : `${preview.montoModificador > 0 ? '+' : ''}$${formatMonto(preview.montoModificador)}`}
                            </Text>
                          </Group>
                          <Group justify="space-between">
                            <Text size="sm" fw={600}>
                              Precio final (paga el cliente)
                            </Text>
                            <Text size="sm" fw={600}>
                              ${formatMonto(preview.precioFinal)}
                            </Text>
                          </Group>
                          {puedeVerGananciaNeta && (
                            <>
                              <Group justify="space-between">
                                <Text size="xs" c="dimmed">
                                  Comisión del medio de pago
                                </Text>
                                <Text size="xs">-${formatMonto(preview.montoComision)}</Text>
                              </Group>
                              <Group justify="space-between">
                                <Text size="sm" fw={600} c="teal">
                                  Ganancia neta
                                </Text>
                                <Text size="sm" fw={600} c="teal">
                                  ${formatMonto(preview.gananciaNeta)}
                                </Text>
                              </Group>
                            </>
                          )}
                        </div>
                      )}

                      {errorMessage && (
                        <Text size="sm" c="red">
                          {errorMessage}
                        </Text>
                      )}

                      <Group gap="xs">
                        <Button type="submit" loading={guardando} flex={1}>
                          {cobroEditandoId ? 'Guardar cambios' : 'Registrar cobro'}
                        </Button>
                        {cobroEditandoId && (
                          <ActionIcon variant="light" color="gray" size="lg" aria-label="Cancelar edición" onClick={abrirNuevo}>
                            <IconX size={16} />
                          </ActionIcon>
                        )}
                      </Group>
                    </Stack>
                  </form>
                )}
              </>
            )}
          </>
        )}
      </Stack>
    </Modal>
  );
}
