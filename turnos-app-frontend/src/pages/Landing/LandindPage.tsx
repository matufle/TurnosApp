import { AppShell, Container, Group, Title, Button, Text, Stack, SimpleGrid, ThemeIcon, rem } from '@mantine/core';
import { IconCalendarCheck, IconUsers, IconBellRinging } from '@tabler/icons-react';
import { Link } from 'react-router-dom';

export function LandingPage() {
  return (
    <AppShell header={{ height: 64 }} padding={0}>
      <AppShell.Header withBorder={false} style={{ backgroundColor: 'white' }}>
        <Container size="lg" h="100%">
          <Group h="100%" justify="space-between">
            <Title order={3} c="cyan.6">
              Turnify
            </Title>
            <Group gap="sm">
              <Button variant="subtle" component={Link} to="/login">
                Iniciar sesión
              </Button>
              <Button component={Link} to="/registro">
                Registrar mi negocio
              </Button>
            </Group>
          </Group>
        </Container>
      </AppShell.Header>

      <AppShell.Main>
        {/* Hero */}
        <Container size="md" py={rem(120)}>
          <Stack align="center" ta="center" gap="xl">
            <Title order={1} fz={{ base: rem(36), sm: rem(52) }} fw={800} maw={640}>
              Simplificá tus turnos con{' '}
              <Text component="span" c="cyan.6" inherit>
                Turnify
              </Text>
            </Title>
            <Text size="lg" c="dimmed" maw={520}>
              Gestioná reservas, recursos y clientes desde un solo lugar. Sin planillas, sin
              llamados perdidos.
            </Text>
            <Button size="lg" component={Link} to="/registro" mt="md">
              Registrar mi negocio gratis
            </Button>
          </Stack>
        </Container>

        {/* Características destacadas */}
        <Container size="lg" py={rem(80)}>
          <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="xl">
            <FeatureItem
              icon={<IconCalendarCheck size={22} />}
              title="Agenda sin fricción"
              description="Tus clientes reservan online, vos ves todo en una sola vista."
            />
            <FeatureItem
              icon={<IconUsers size={22} />}
              title="Multi-negocio"
              description="Administrá varias sucursales o profesionales desde una misma cuenta."
            />
            <FeatureItem
              icon={<IconBellRinging size={22} />}
              title="Recordatorios automáticos"
              description="Reducí ausencias con avisos antes de cada turno."
            />
          </SimpleGrid>
        </Container>
      </AppShell.Main>
    </AppShell>
  );
}

function FeatureItem({ icon, title, description }: { icon: React.ReactNode; title: string; description: string }) {
  return (
    <Stack align="center" ta="center" gap="xs">
      <ThemeIcon size={44} radius="md" variant="light">
        {icon}
      </ThemeIcon>
      <Text fw={600}>{title}</Text>
      <Text size="sm" c="dimmed">
        {description}
      </Text>
    </Stack>
  );
}