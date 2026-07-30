// src/layout/DashboardLayout.tsx
import { useEffect, useState } from 'react'; // 👈 Importamos useState
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { 
  AppShell, 
  Burger, 
  Group, 
  NavLink, 
  Avatar, 
  Text, 
  Menu, 
  UnstyledButton, 
  Stack,
  Center, // 👈 Importamos Center
  Loader  // 👈 Importamos Loader
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import {
  IconChevronDown,
  IconLogout,
  IconLayoutDashboard,
  IconSettings,
} from '@tabler/icons-react';

// Servicios y Contextos
import { tenantService } from '../api/tenantService';
import { useTenantTheme } from '../context/useTenantTheme';
import { useAuth } from '../context/useAuth';
import { OnboardingTour } from '../components/OnboardingTour/OnboardingTour';
import { navLinks, esLinkVisible } from './navLinks';

export function DashboardLayout() {
  const [opened, { toggle }] = useDisclosure();
  const location = useLocation();
  const navigate = useNavigate();
  
  // 1. Traemos tanto el color actual como la función para actualizarlo
  const { colorHex, setColorHex } = useTenantTheme();
  const { user, hasPermission, logout } = useAuth();
  const navLinksVisibles = navLinks.filter((link) => esLinkVisible(link.permiso, hasPermission));

  // 2. Creamos un estado de carga inteligente.
  // Si 'colorHex' ya existe (ej. al presionar F5 en una página privada), no mostramos carga.
  // Si venimos del login ('colorHex' es null), mostramos carga hasta que el fetch termine.
  const [loading, setLoading] = useState(!colorHex);

  useEffect(() => {
    const cargarColorAlEntrar = async () => {
      // Si el color ya fue cargado por App.tsx, no hacemos nada y evitamos doble loader
      if (colorHex) {
        setLoading(false);
        return;
      }

      try {
        const config = await tenantService.getConfig();
        if (config.colorPrimario) {
          setColorHex(config.colorPrimario);
        }
      } catch (error) {
        console.warn('Error al cargar el color de la marca en el layout', error);
      } finally {
        setLoading(false);
      }
    };

    cargarColorAlEntrar();
  }, [colorHex, setColorHex]);

  // 3. Mientras se resuelve el color al venir desde el Login, mostramos un spinner limpio.
  // De esta manera, el AppShell solo se renderizará cuando Mantine ya tenga tu verde/dorado aplicado.
  if (loading) {
    return (
      <Center h="100vh">
        <Loader type="dots" color="cyan" />
      </Center>
    );
  }

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{
        width: 250,
        breakpoint: 'sm',
        collapsed: { mobile: !opened }
      }}
      padding="md"
    >
      <OnboardingTour />

      {/* 🟢 HEADER */}
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Group>
            <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
            <Text size="xl" fw={800} c="brand">
              Turnify
            </Text>
          </Group>

          <Menu width={200} position="bottom-end" shadow="md">
            <Menu.Target>
              <UnstyledButton>
                <Group gap={7}>
                  <Avatar radius="xl" size="sm" color="brand">
                    {(user?.nombre?.[0] ?? '?').toUpperCase()}
                  </Avatar>
                  <Text fw={500} size="sm">{user?.nombre ?? 'Mi cuenta'}</Text>
                  <IconChevronDown size={14} stroke={1.5} />
                </Group>
              </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
              <Menu.Item
                leftSection={<IconLayoutDashboard size={14} />}
                onClick={() => navigate('/app')}
              >
                Dashboard
              </Menu.Item>
              <Menu.Item leftSection={<IconSettings size={14} />}>
                Mi Perfil
              </Menu.Item>
              <Menu.Item
                color="red"
                leftSection={<IconLogout size={14} />}
                onClick={() => {
                  logout();
                  navigate('/login');
                }}
              >
                Cerrar sesión
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </AppShell.Header>

      {/* 🟢 NAVBAR */}
      <AppShell.Navbar p="md">
        <Stack gap="xs">
          {navLinksVisibles.map((link) => (
            <NavLink
              key={link.path}
              data-tour={`nav-${link.path.split('/').pop()}`}
              label={link.label}
              leftSection={<link.icon size={20} stroke={1.5} />}
              active={location.pathname.startsWith(link.path)}
              onClick={() => {
                navigate(link.path);
                if (opened) toggle();
              }}
              color="brand"
              variant="light"
              style={{ borderRadius: 8 }}
            />
          ))}
        </Stack>
      </AppShell.Navbar>

      {/* 🟢 CONTENIDO PRINCIPAL */}
      <AppShell.Main bg="gray.0">
        <div style={{ maxWidth: 1200, margin: '0 auto' }}>
          <Outlet />
        </div>
      </AppShell.Main>
    </AppShell>
  );
}