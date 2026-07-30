// src/pages/Caja/DetalleSesionModal.tsx
import { useEffect, useState } from 'react';
import { Modal, Stack, Text, Group, Divider, Badge, Table } from '@mantine/core';
import { cajaService } from '../../api/cajaService';
import { PageSpinner } from '../../components/PageSpinner';
import type { SesionCaja } from '../../types/Caja';

interface DetalleSesionModalProps {
  opened: boolean;
  onClose: () => void;
  sesionId: number;
}

function formatMonto(valor: number) {
  return valor.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatFechaHora(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
}

export function DetalleSesionModal({ opened, onClose, sesionId }: DetalleSesionModalProps) {
  const [sesion, setSesion] = useState<SesionCaja | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!opened) return;
    setLoading(true);
    cajaService
      .getById(sesionId)
      .then(setSesion)
      .finally(() => setLoading(false));
  }, [opened, sesionId]);

  const diferencia = sesion?.diferencia ?? null;
  const otrosMedios = sesion?.desglosePorMedioPago.filter((d) => !d.esEfectivo) ?? [];

  return (
    <Modal opened={opened} onClose={onClose} title="Detalle de la sesión de caja" centered size="lg">
      {loading || !sesion ? (
        <PageSpinner size="sm" />
      ) : (
        <Stack gap="md">
          <Group justify="space-between">
            <Text size="sm" c="dimmed">
              Apertura
            </Text>
            <Text size="sm">
              {formatFechaHora(sesion.fechaApertura)} — {sesion.usuarioAperturaNombre}
            </Text>
          </Group>
          <Group justify="space-between">
            <Text size="sm" c="dimmed">
              Cierre
            </Text>
            <Group gap="xs">
              <Text size="sm">
                {sesion.fechaCierre ? `${formatFechaHora(sesion.fechaCierre)} — ${sesion.usuarioCierreNombre}` : '—'}
              </Text>
              {sesion.cierreForzado && (
                <Badge color="orange" variant="light">
                  Cierre forzado
                </Badge>
              )}
            </Group>
          </Group>

          <Divider />

          <Group justify="space-between">
            <Text size="sm" c="dimmed">
              Monto inicial
            </Text>
            <Text size="sm">${formatMonto(sesion.montoInicial)}</Text>
          </Group>
          <Group justify="space-between">
            <Text size="sm" c="dimmed">
              Efectivo esperado
            </Text>
            <Text size="sm">${formatMonto(sesion.montoEsperadoEfectivo)}</Text>
          </Group>
          <Group justify="space-between">
            <Text size="sm" c="dimmed">
              Efectivo declarado
            </Text>
            <Text size="sm">{sesion.montoFinalDeclarado !== null ? `$${formatMonto(sesion.montoFinalDeclarado)}` : '—'}</Text>
          </Group>
          <Group justify="space-between">
            <Text size="sm" fw={600}>
              Diferencia
            </Text>
            <Text size="sm" fw={700} c={diferencia === null || diferencia === 0 ? undefined : diferencia > 0 ? 'teal' : 'red'}>
              {diferencia === null ? '—' : diferencia === 0 ? 'Sin diferencia' : `${diferencia > 0 ? '+' : ''}$${formatMonto(diferencia)}`}
            </Text>
          </Group>

          {otrosMedios.length > 0 && (
            <>
              <Divider label="Otros medios de pago (informativo)" labelPosition="left" />
              {otrosMedios.map((d) => (
                <Group key={`${d.metodoPagoId ?? 'sin-metodo'}-${d.nombre}`} justify="space-between">
                  <Text size="sm" c="dimmed">
                    {d.nombre}
                  </Text>
                  <Text size="sm">${formatMonto(d.total)}</Text>
                </Group>
              ))}
            </>
          )}

          {sesion.observaciones && (
            <>
              <Divider label="Observaciones" labelPosition="left" />
              <Text size="sm">{sesion.observaciones}</Text>
            </>
          )}

          <Divider label={`Movimientos (${sesion.movimientos.length})`} labelPosition="left" />
          <Table.ScrollContainer minWidth={500} mah={300} style={{ overflowY: 'auto' }}>
            <Table verticalSpacing="xs" horizontalSpacing="sm">
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Fecha</Table.Th>
                  <Table.Th>Tipo</Table.Th>
                  <Table.Th>Método</Table.Th>
                  <Table.Th>Concepto</Table.Th>
                  <Table.Th>Usuario</Table.Th>
                  <Table.Th ta="right">Monto</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {sesion.movimientos.map((m) => (
                  <Table.Tr key={m.id}>
                    <Table.Td>{formatFechaHora(m.fechaHora)}</Table.Td>
                    <Table.Td>
                      <Badge size="sm" color={m.tipo === 'Ingreso' ? 'green' : 'red'} variant="light">
                        {m.tipo}
                      </Badge>
                    </Table.Td>
                    <Table.Td>{m.nombreMetodoPagoSnapshot}</Table.Td>
                    <Table.Td>{m.concepto}</Table.Td>
                    <Table.Td>{m.usuarioNombre}</Table.Td>
                    <Table.Td ta="right">
                      {m.tipo === 'Ingreso' ? '+' : '-'}${formatMonto(m.monto)}
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        </Stack>
      )}
    </Modal>
  );
}
