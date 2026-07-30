// src/pages/Caja/CerrarCajaModal.tsx
import { useState } from 'react';
import axios from 'axios';
import { Modal, NumberInput, Textarea, Button, Stack, Text, Group, Divider } from '@mantine/core';
import { useForm } from '@mantine/form';
import { cajaService } from '../../api/cajaService';
import type { SesionCaja } from '../../types/Caja';

interface CerrarCajaModalProps {
  opened: boolean;
  onClose: () => void;
  sesion: SesionCaja;
  onSesionCerrada: () => void | Promise<void>;
}

interface CerrarFormValues {
  montoFinalDeclarado: number;
  observaciones: string;
}

function formatMonto(valor: number) {
  return valor.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

export function CerrarCajaModal({ opened, onClose, sesion, onSesionCerrada }: CerrarCajaModalProps) {
  const [guardando, setGuardando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm<CerrarFormValues>({
    initialValues: { montoFinalDeclarado: Number(sesion.montoEsperadoEfectivo.toFixed(2)), observaciones: '' },
    validate: {
      montoFinalDeclarado: (value) => (value >= 0 ? null : 'No puede ser negativo'),
    },
  });

  const diferencia = form.values.montoFinalDeclarado - sesion.montoEsperadoEfectivo;
  const otrosMedios = sesion.desglosePorMedioPago.filter((d) => !d.esEfectivo);

  const handleSubmit = async (values: CerrarFormValues) => {
    setGuardando(true);
    setErrorMessage(null);
    try {
      await cajaService.cerrar(sesion.id, {
        montoFinalDeclarado: values.montoFinalDeclarado,
        observaciones: values.observaciones || undefined,
      });
      form.reset();
      onClose();
      await onSesionCerrada();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos cerrar la caja.');
      }
    } finally {
      setGuardando(false);
    }
  };

  return (
    <Modal
      opened={opened}
      onClose={() => {
        onClose();
        setErrorMessage(null);
      }}
      title="Cerrar caja"
      centered
      size="md"
    >
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <Group justify="space-between">
            <Text size="sm" c="dimmed">
              Efectivo esperado (inicial + movimientos en efectivo)
            </Text>
            <Text size="sm" fw={600}>
              ${formatMonto(sesion.montoEsperadoEfectivo)}
            </Text>
          </Group>

          <NumberInput
            label="Efectivo contado (declarado)"
            min={0}
            decimalScale={2}
            prefix="$"
            {...form.getInputProps('montoFinalDeclarado')}
          />

          <div
            className={`rounded-xl p-3 flex justify-between items-center ${
              diferencia === 0
                ? 'bg-surface-container-low'
                : diferencia > 0
                  ? 'bg-tertiary-container/40'
                  : 'bg-error-container'
            }`}
          >
            <span className="font-body-sm text-body-sm font-semibold text-on-surface">Diferencia</span>
            <span
              className={`font-title-md text-body-lg font-bold ${
                diferencia === 0 ? 'text-secondary' : diferencia > 0 ? 'text-tertiary-container' : 'text-on-error-container'
              }`}
            >
              {diferencia === 0 ? 'Sin diferencia' : `${diferencia > 0 ? '+' : ''}$${formatMonto(diferencia)}`}
            </span>
          </div>

          {otrosMedios.length > 0 && (
            <>
              <Divider label="Otros medios de pago (informativo, no se cuentan físicamente)" labelPosition="left" />
              <Stack gap={4}>
                {otrosMedios.map((d) => (
                  <Group key={`${d.metodoPagoId ?? 'sin-metodo'}-${d.nombre}`} justify="space-between">
                    <Text size="sm" c="dimmed">
                      {d.nombre}
                    </Text>
                    <Text size="sm">${formatMonto(d.total)}</Text>
                  </Group>
                ))}
              </Stack>
            </>
          )}

          <Textarea label="Observaciones" placeholder="Opcional" {...form.getInputProps('observaciones')} />

          {errorMessage && (
            <Text size="sm" c="red">
              {errorMessage}
            </Text>
          )}

          <Button type="submit" loading={guardando} fullWidth mt="sm" color="red">
            Confirmar cierre
          </Button>
        </Stack>
      </form>
    </Modal>
  );
}
