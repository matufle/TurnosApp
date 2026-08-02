// src/pages/Suscripcion/SuscripcionPage.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Text,
  Stack,
  Paper,
  Badge,
  Button,
  Alert,
  Loader,
  Center,
  Group,
  Divider,
} from '@mantine/core';
import { IconAlertCircle, IconCrown } from '@tabler/icons-react';
import { suscripcionService } from '../../api/suscripcionService';
import { usePermission } from '../../hooks/usePermission';
import { formatMonto } from '../../utils/format';
import type { Suscripcion, EstadoSuscripcion } from '../../types/Suscripcion';

const COLOR_POR_ESTADO: Record<EstadoSuscripcion, string> = {
  Trial: 'blue',
  Activa: 'green',
  PastDue: 'orange',
  Cancelada: 'red',
};

const LABEL_POR_ESTADO: Record<EstadoSuscripcion, string> = {
  Trial: 'Período de prueba',
  Activa: 'Activa',
  PastDue: 'Pago pendiente',
  Cancelada: 'Cancelada',
};

export function SuscripcionPage() {
  const puedeGestionar = usePermission('GestionarSuscripcion');

  const [suscripcion, setSuscripcion] = useState<Suscripcion | null>(null);
  const [loading, setLoading] = useState(true);
  const [procesando, setProcesando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const cargarEstado = async () => {
    try {
      const data = await suscripcionService.getEstado();
      setSuscripcion(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar el estado de tu suscripción.');
    }
  };

  useEffect(() => {
    cargarEstado().finally(() => setLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const manejarError = (error: unknown, fallback: string) => {
    if (axios.isAxiosError(error) && error.response?.data?.detail) {
      setErrorMessage(error.response.data.detail);
    } else {
      setErrorMessage(fallback);
    }
  };

  const iniciarSuscripcion = async () => {
    setProcesando(true);
    setErrorMessage(null);
    try {
      const url = await suscripcionService.iniciar();
      window.location.href = url;
    } catch (error) {
      manejarError(error, 'No pudimos iniciar la suscripción con Mercado Pago. Intentá de nuevo.');
      setProcesando(false);
    }
  };

  const cancelarSuscripcion = async () => {
    if (!window.confirm('¿Seguro que querés cancelar la suscripción?')) return;

    setProcesando(true);
    setErrorMessage(null);
    try {
      await suscripcionService.cancelar();
      await cargarEstado();
    } catch (error) {
      manejarError(error, 'No pudimos cancelar la suscripción. Intentá de nuevo.');
    } finally {
      setProcesando(false);
    }
  };

  if (loading) {
    return (
      <Center py="xl">
        <Loader />
      </Center>
    );
  }

  return (
    <Stack gap="lg" maw={600}>
      <div>
        <Title order={2}>Suscripción</Title>
        <Text c="dimmed" size="sm">
          Estado de la suscripción de tu negocio a Slotia.
        </Text>
      </div>

      {errorMessage && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" variant="light">
          {errorMessage}
        </Alert>
      )}

      {suscripcion && (
        <Paper withBorder radius="md" p="xl">
          <Stack gap="md">
            <Group justify="space-between" align="center">
              <Group gap="xs">
                <IconCrown size={20} />
                <Text fw={600}>{suscripcion.planNombre ?? 'Sin plan asignado'}</Text>
              </Group>
              {suscripcion.esGrandfathered ? (
                <Badge color="grape" variant="light">Acceso ilimitado</Badge>
              ) : (
                <Badge color={COLOR_POR_ESTADO[suscripcion.estadoSuscripcion]} variant="light">
                  {LABEL_POR_ESTADO[suscripcion.estadoSuscripcion]}
                </Badge>
              )}
            </Group>

            {suscripcion.planPrecioMensual != null && (
              <Text size="sm" c="dimmed">
                ARS ${formatMonto(suscripcion.planPrecioMensual)} / mes
              </Text>
            )}

            {suscripcion.esGrandfathered ? (
              <Text size="sm" c="dimmed">
                Tu cuenta tiene acceso completo sin cargo, sin necesidad de suscribirte.
              </Text>
            ) : (
              <>
                {suscripcion.estadoSuscripcion === 'Trial' && suscripcion.suscripcionVenceEn && (
                  <Text size="sm" c="dimmed">
                    Tu período de prueba vence el{' '}
                    {new Date(suscripcion.suscripcionVenceEn).toLocaleDateString('es-AR')}.
                  </Text>
                )}

                {suscripcion.estadoSuscripcion === 'PastDue' && (
                  <Text size="sm" c="orange">
                    Tu suscripción está pausada por Mercado Pago. Iniciá una suscripción nueva para reactivar el acceso.
                  </Text>
                )}

                {suscripcion.estadoSuscripcion === 'Cancelada' && (
                  <Text size="sm" c="red">
                    Tu suscripción está cancelada. Suscribite de nuevo para seguir usando Slotia.
                  </Text>
                )}

                <Divider />

                {puedeGestionar ? (
                  <Group>
                    {suscripcion.estadoSuscripcion === 'Activa' ? (
                      <Button color="red" variant="light" loading={procesando} onClick={cancelarSuscripcion}>
                        Cancelar suscripción
                      </Button>
                    ) : (
                      <Button loading={procesando} onClick={iniciarSuscripcion}>
                        Suscribirme con Mercado Pago
                      </Button>
                    )}
                  </Group>
                ) : (
                  <Text size="sm" c="dimmed">
                    Necesitás el permiso "Gestionar suscripción" para administrar el pago del negocio.
                  </Text>
                )}
              </>
            )}
          </Stack>
        </Paper>
      )}
    </Stack>
  );
}
