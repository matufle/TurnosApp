// src/pages/Reservas/ReservarPage.tsx
// Detrás de ClienteProtectedRoute: reserva obligatoria, como pide el enunciado original.
// Simplificación deliberada de esta primera versión: un solo servicio por turno (no el
// multi-servicio que sí soporta el flujo de staff en TurnosPage.tsx).
import { useEffect, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { Alert, Button, Center, Group, Loader, Select, SimpleGrid, Stack, Text, Title } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import axios from 'axios';
import { publicCatalogoService } from '../../api/publicCatalogoService';
import { misTurnosService } from '../../api/misTurnosService';
import { horaUtcALocal } from '../../utils/horarioTimezone';
import type { Servicio } from '../../types/Servicio';
import type { RecursoPublico } from '../../types/ReservaPublica';

export function ReservarPage() {
  const { slug } = useParams<{ slug: string }>();
  const [searchParams] = useSearchParams();
  const servicioId = Number(searchParams.get('servicioId'));
  const navigate = useNavigate();

  const [servicio, setServicio] = useState<Servicio | null>(null);
  const [recursos, setRecursos] = useState<RecursoPublico[]>([]);
  const [recursoId, setRecursoId] = useState<string | null>(null);
  const [fecha, setFecha] = useState<string | null>(null);
  const [slotsUtc, setSlotsUtc] = useState<string[]>([]);
  const [cargandoSlots, setCargandoSlots] = useState(false);
  const [cargandoInicial, setCargandoInicial] = useState(true);
  const [slotConfirmando, setSlotConfirmando] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [errorCarga, setErrorCarga] = useState(false);

  useEffect(() => {
    if (!slug || !servicioId) return;

    setCargandoInicial(true);
    setErrorCarga(false);

    Promise.all([publicCatalogoService.getServicios(slug), publicCatalogoService.getRecursos(slug)])
      .then(([servicios, recursosData]) => {
        setServicio(servicios.find((s) => s.id === servicioId) ?? null);
        setRecursos(recursosData);
        if (recursosData.length > 0) setRecursoId(String(recursosData[0].id));
      })
      .catch(() => setErrorCarga(true))
      .finally(() => setCargandoInicial(false));
  }, [slug, servicioId]);

  useEffect(() => {
    if (!slug || !recursoId || !fecha) {
      setSlotsUtc([]);
      return;
    }

    setCargandoSlots(true);
    publicCatalogoService
      .getDisponibilidad(slug, Number(recursoId), servicioId, fecha)
      .then(setSlotsUtc)
      .finally(() => setCargandoSlots(false));
  }, [slug, recursoId, fecha, servicioId]);

  const confirmar = async (slotUtc: string) => {
    if (!slug || !recursoId || !fecha) return;

    setErrorMessage(null);
    setSlotConfirmando(slotUtc);

    try {
      // slotUtc ya viene en la misma convención "UTC-equivalente" que usa el backend —
      // combinarlo directo con la fecha arma el instante UTC correcto (ver horarioTimezone.ts).
      await misTurnosService.crear({
        recursoId: Number(recursoId),
        servicioIds: [servicioId],
        fechaHoraInicio: `${fecha}T${slotUtc}:00.000Z`,
      });
      navigate(`/reservas/${slug}/mis-turnos`, { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        setErrorMessage('Ese horario ya no está disponible. Elegí otro.');
        setSlotsUtc((prev) => prev.filter((s) => s !== slotUtc));
      } else {
        setErrorMessage('No pudimos confirmar la reserva. Intentá de nuevo.');
      }
    } finally {
      setSlotConfirmando(null);
    }
  };

  if (!servicioId) {
    return (
      <Center py="xl">
        <Text c="dimmed">Elegí un servicio desde el catálogo para reservar.</Text>
      </Center>
    );
  }

  if (cargandoInicial) {
    return (
      <Center py="xl">
        <Loader type="dots" />
      </Center>
    );
  }

  if (errorCarga) {
    return (
      <Center py="xl">
        <Stack align="center" gap="sm">
          <Text c="dimmed">No pudimos cargar los datos de reserva. Intentá de nuevo.</Text>
          <Button variant="light" onClick={() => window.location.reload()}>
            Reintentar
          </Button>
        </Stack>
      </Center>
    );
  }

  return (
    <Stack maw={420} w="100%" mx="auto" py="xl" px="md" gap="md">
      <Title order={3}>Reservar {servicio?.nombre ?? ''}</Title>

      {recursos.length > 1 && (
        <Select
          label="Con quién"
          data={recursos.map((r) => ({ value: String(r.id), label: r.nombre }))}
          value={recursoId}
          onChange={setRecursoId}
        />
      )}

      <DatePickerInput label="Fecha" value={fecha} onChange={setFecha} minDate={new Date()} />

      {errorMessage && <Alert color="red">{errorMessage}</Alert>}

      {cargandoSlots && (
        <Center py="md">
          <Loader type="dots" />
        </Center>
      )}

      {!cargandoSlots && recursoId && fecha && slotsUtc.length === 0 && (
        <Text c="dimmed">No hay horarios disponibles ese día.</Text>
      )}

      {!cargandoSlots && slotsUtc.length > 0 && (
        <SimpleGrid cols={{ base: 3, xs: 4 }} spacing="xs">
          {slotsUtc.map((slotUtc) => (
            <Button
              key={slotUtc}
              variant="light"
              size="sm"
              loading={slotConfirmando === slotUtc}
              disabled={slotConfirmando !== null && slotConfirmando !== slotUtc}
              onClick={() => confirmar(slotUtc)}
            >
              {horaUtcALocal(slotUtc)}
            </Button>
          ))}
        </SimpleGrid>
      )}

      <Group justify="center">
        <Text size="xs" c="dimmed">
          Los horarios se muestran en tu hora local.
        </Text>
      </Group>
    </Stack>
  );
}
