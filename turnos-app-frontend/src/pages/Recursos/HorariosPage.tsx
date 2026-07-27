// src/pages/Recursos/HorariosPage.tsx
// Horario de atención semanal por Recurso, usado por el motor de disponibilidad de la
// reserva pública (/reservas/:slug). Página standalone: no toca el modal existente de
// RecursosPage.tsx, para no arriesgar una regresión ahí.
import { useEffect, useState } from 'react';
import { ActionIcon, Alert, Button, Group, Select, Stack, Table, Title } from '@mantine/core';
import { TimeInput } from '@mantine/dates';
import { IconPlus, IconTrash } from '@tabler/icons-react';
import { recursosService } from '../../api/recursosService';
import { horariosAtencionService } from '../../api/horariosAtencionService';
import { horaLocalAUtc, horaUtcALocal } from '../../utils/horarioTimezone';
import { NOMBRES_DIAS, type DiaSemana } from '../../types/HorarioAtencion';
import type { Recurso } from '../../types/Recurso';

interface FilaHorario {
  diaSemana: DiaSemana;
  horaInicio: string; // local "HH:mm", solo para edición en pantalla
  horaFin: string;
}

const DIAS_OPCIONES = ([1, 2, 3, 4, 5, 6, 0] as DiaSemana[]).map((d) => ({
  value: String(d),
  label: NOMBRES_DIAS[d],
}));

export function HorariosPage() {
  const [recursos, setRecursos] = useState<Recurso[]>([]);
  const [recursoId, setRecursoId] = useState<string | null>(null);
  const [filas, setFilas] = useState<FilaHorario[]>([]);
  const [loading, setLoading] = useState(false);
  const [guardando, setGuardando] = useState(false);
  const [mensaje, setMensaje] = useState<{ tipo: 'success' | 'error'; texto: string } | null>(null);

  useEffect(() => {
    recursosService.getAll().then(setRecursos);
  }, []);

  useEffect(() => {
    if (!recursoId) {
      setFilas([]);
      return;
    }

    setLoading(true);
    horariosAtencionService
      .getByRecurso(Number(recursoId))
      .then((horarios) => {
        setFilas(
          horarios.map((h) => ({
            diaSemana: h.diaSemana,
            horaInicio: horaUtcALocal(h.horaInicio.slice(0, 5)),
            horaFin: horaUtcALocal(h.horaFin.slice(0, 5)),
          }))
        );
      })
      .finally(() => setLoading(false));
  }, [recursoId]);

  const agregarFila = () => {
    setFilas((prev) => [...prev, { diaSemana: 1, horaInicio: '09:00', horaFin: '18:00' }]);
  };

  const quitarFila = (index: number) => {
    setFilas((prev) => prev.filter((_, i) => i !== index));
  };

  const actualizarFila = (index: number, cambios: Partial<FilaHorario>) => {
    setFilas((prev) => prev.map((f, i) => (i === index ? { ...f, ...cambios } : f)));
  };

  const guardar = async () => {
    if (!recursoId) return;

    setMensaje(null);
    setGuardando(true);

    try {
      await horariosAtencionService.reemplazar(
        Number(recursoId),
        filas.map((f) => ({
          diaSemana: f.diaSemana,
          horaInicio: `${horaLocalAUtc(f.horaInicio)}:00`,
          horaFin: `${horaLocalAUtc(f.horaFin)}:00`,
        }))
      );
      setMensaje({ tipo: 'success', texto: 'Horario guardado.' });
    } catch {
      setMensaje({ tipo: 'error', texto: 'No se pudo guardar el horario. Revisá los rangos ingresados.' });
    } finally {
      setGuardando(false);
    }
  };

  return (
    <Stack gap="md">
      <Title order={2}>Horarios de atención</Title>

      <Select
        label="Recurso"
        placeholder="Elegí un recurso"
        data={recursos.map((r) => ({ value: String(r.id), label: r.nombre }))}
        value={recursoId}
        onChange={setRecursoId}
        maw={320}
        w="100%"
      />

      {recursoId && !loading && (
        <Stack gap="sm">
          {mensaje && <Alert color={mensaje.tipo === 'success' ? 'green' : 'red'}>{mensaje.texto}</Alert>}

          <Table.ScrollContainer minWidth={420}>
            <Table>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Día</Table.Th>
                  <Table.Th>Desde</Table.Th>
                  <Table.Th>Hasta</Table.Th>
                  <Table.Th />
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {filas.map((fila, index) => (
                  <Table.Tr key={index}>
                    <Table.Td>
                      <Select
                        data={DIAS_OPCIONES}
                        value={String(fila.diaSemana)}
                        onChange={(v) => actualizarFila(index, { diaSemana: Number(v) as DiaSemana })}
                        w={140}
                      />
                    </Table.Td>
                    <Table.Td>
                      <TimeInput
                        value={fila.horaInicio}
                        onChange={(e) => actualizarFila(index, { horaInicio: e.currentTarget.value })}
                        w={110}
                      />
                    </Table.Td>
                    <Table.Td>
                      <TimeInput
                        value={fila.horaFin}
                        onChange={(e) => actualizarFila(index, { horaFin: e.currentTarget.value })}
                        w={110}
                      />
                    </Table.Td>
                    <Table.Td>
                      <ActionIcon color="red" variant="subtle" onClick={() => quitarFila(index)}>
                        <IconTrash size={16} />
                      </ActionIcon>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>

          <Group>
            <Button variant="light" leftSection={<IconPlus size={16} />} onClick={agregarFila}>
              Agregar bloque
            </Button>
            <Button onClick={guardar} loading={guardando}>
              Guardar
            </Button>
          </Group>
        </Stack>
      )}
    </Stack>
  );
}
