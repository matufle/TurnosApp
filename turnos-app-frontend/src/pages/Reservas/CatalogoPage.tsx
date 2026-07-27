// src/pages/Reservas/CatalogoPage.tsx
// Ruta índice de /reservas/:slug — catálogo público de solo lectura (anónimo).
import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { Button, Card, Center, Group, Loader, Stack, Text, Title } from '@mantine/core';
import { publicCatalogoService } from '../../api/publicCatalogoService';
import type { Servicio } from '../../types/Servicio';

export function CatalogoPage() {
  const { slug } = useParams<{ slug: string }>();
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!slug) return;
    publicCatalogoService.getServicios(slug).then((data) => {
      setServicios(data);
      setLoading(false);
    });
  }, [slug]);

  if (loading) {
    return (
      <Center py="xl">
        <Loader type="dots" />
      </Center>
    );
  }

  return (
    <Stack maw={480} w="100%" mx="auto" py="xl" px="md" gap="md">
      <Title order={3}>Servicios</Title>

      {servicios.length === 0 && <Text c="dimmed">Todavía no hay servicios disponibles.</Text>}

      {servicios.map((servicio) => (
        <Card key={servicio.id} withBorder padding="md">
          <Group justify="space-between" align="center">
            <div>
              <Text fw={600}>{servicio.nombre}</Text>
              <Text size="sm" c="dimmed">
                {servicio.duracionMinutos} min — ${servicio.precio}
              </Text>
            </div>

            <Button component={Link} to={`/reservas/${slug}/reservar?servicioId=${servicio.id}`}>
              Reservar
            </Button>
          </Group>
        </Card>
      ))}

      <Text ta="center" size="sm">
        ¿Ya reservaste antes? <Link to={`/reservas/${slug}/mis-turnos`}>Ver mis turnos</Link>
      </Text>
    </Stack>
  );
}
