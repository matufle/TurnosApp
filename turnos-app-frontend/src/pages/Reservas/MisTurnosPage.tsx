// src/pages/Reservas/MisTurnosPage.tsx
import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { Badge, Button, Card, Center, Group, Loader, Stack, Text, Title } from '@mantine/core';
import { misTurnosService } from '../../api/misTurnosService';
import { useClienteAuth } from '../../context/useClienteAuth';
import type { Turno } from '../../types/Turno';

export function MisTurnosPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { logout } = useClienteAuth();
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [loading, setLoading] = useState(true);
  const [cancelandoId, setCancelandoId] = useState<number | null>(null);

  const cargarTurnos = useCallback(async () => {
    setLoading(true);
    try {
      const data = await misTurnosService.getMisTurnos();
      setTurnos(data);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarTurnos();
  }, [cargarTurnos]);

  const handleCancelar = async (id: number) => {
    setCancelandoId(id);
    try {
      await misTurnosService.cancelar(id);
      await cargarTurnos();
    } finally {
      setCancelandoId(null);
    }
  };

  const handleLogout = () => {
    logout();
    navigate(`/reservas/${slug}/login`, { replace: true });
  };

  if (loading) {
    return (
      <Center py="xl">
        <Loader type="dots" />
      </Center>
    );
  }

  return (
    <Stack maw={480} w="100%" mx="auto" py="xl" px="md" gap="md">
      <Group justify="space-between">
        <Title order={3}>Mis turnos</Title>
        <Button variant="subtle" onClick={handleLogout}>
          Cerrar sesión
        </Button>
      </Group>

      <Button component={Link} to={`/reservas/${slug}`} variant="light">
        Reservar otro turno
      </Button>

      {turnos.length === 0 && <Text c="dimmed">Todavía no tenés turnos.</Text>}

      {turnos.map((turno) => (
        <Card key={turno.id} withBorder padding="md">
          <Group justify="space-between" align="flex-start">
            <div>
              <Text fw={600}>{turno.servicios.join(', ')}</Text>
              <Text size="sm" c="dimmed">
                {new Date(turno.fechaHoraInicio).toLocaleString('es-AR')} — {turno.recursoNombre}
              </Text>
              <Badge mt="xs" color={turno.estado === 'Cancelado' ? 'gray' : 'blue'}>
                {turno.estado}
              </Badge>
            </div>

            {turno.estado !== 'Cancelado' && (
              <Button
                color="red"
                variant="light"
                size="xs"
                loading={cancelandoId === turno.id}
                onClick={() => handleCancelar(turno.id)}
              >
                Cancelar
              </Button>
            )}
          </Group>
        </Card>
      ))}
    </Stack>
  );
}
