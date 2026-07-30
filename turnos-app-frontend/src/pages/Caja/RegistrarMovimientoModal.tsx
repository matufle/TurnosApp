// src/pages/Caja/RegistrarMovimientoModal.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import { Modal, Select, NumberInput, TextInput, Button, Stack, Text } from '@mantine/core';
import { useForm, isNotEmpty } from '@mantine/form';
import { metodosPagoService } from '../../api/metodosPagoService';
import { cajaService } from '../../api/cajaService';
import type { MetodoPago } from '../../types/MetodoPago';
import type { TipoMovimientoCaja } from '../../types/Caja';

interface RegistrarMovimientoModalProps {
  opened: boolean;
  onClose: () => void;
  onMovimientoRegistrado: () => void | Promise<void>;
  tipoInicial?: TipoMovimientoCaja;
}

interface MovimientoFormValues {
  tipo: TipoMovimientoCaja;
  metodoPagoId: string;
  monto: number;
  concepto: string;
}

const TIPO_DATA: { value: TipoMovimientoCaja; label: string }[] = [
  { value: 'Ingreso', label: 'Ingreso' },
  { value: 'Egreso', label: 'Egreso' },
];

export function RegistrarMovimientoModal({
  opened,
  onClose,
  onMovimientoRegistrado,
  tipoInicial = 'Ingreso',
}: RegistrarMovimientoModalProps) {
  const [metodosPago, setMetodosPago] = useState<MetodoPago[]>([]);
  const [guardando, setGuardando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm<MovimientoFormValues>({
    initialValues: { tipo: tipoInicial, metodoPagoId: '', monto: 0, concepto: '' },
    validate: {
      metodoPagoId: isNotEmpty('Elegí un método de pago'),
      monto: (value) => (value > 0 ? null : 'El monto debe ser mayor a cero'),
      concepto: isNotEmpty('Describí el movimiento'),
    },
  });

  useEffect(() => {
    if (!opened) return;
    metodosPagoService.getAll().then((data) => setMetodosPago(data.filter((m) => m.activo)));
    form.setFieldValue('tipo', tipoInicial);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opened, tipoInicial]);

  const handleSubmit = async (values: MovimientoFormValues) => {
    setGuardando(true);
    setErrorMessage(null);
    try {
      await cajaService.registrarMovimiento({
        tipo: values.tipo,
        metodoPagoId: Number(values.metodoPagoId),
        monto: values.monto,
        concepto: values.concepto,
      });
      form.reset();
      onClose();
      await onMovimientoRegistrado();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos registrar el movimiento.');
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
        form.reset();
        setErrorMessage(null);
      }}
      title="Registrar movimiento"
      centered
    >
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <Select label="Tipo" data={TIPO_DATA} allowDeselect={false} {...form.getInputProps('tipo')} />
          <Select
            label="Método de pago"
            placeholder="Elegí un método"
            data={metodosPago.map((m) => ({ value: String(m.id), label: m.nombre }))}
            {...form.getInputProps('metodoPagoId')}
          />
          <NumberInput label="Monto" min={0.01} decimalScale={2} prefix="$" {...form.getInputProps('monto')} />
          <TextInput
            label="Concepto"
            placeholder="Ej: Retiro parcial, pago a proveedor..."
            {...form.getInputProps('concepto')}
          />

          {errorMessage && (
            <Text size="sm" c="red">
              {errorMessage}
            </Text>
          )}

          <Button type="submit" loading={guardando} fullWidth mt="sm">
            Registrar
          </Button>
        </Stack>
      </form>
    </Modal>
  );
}
