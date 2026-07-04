// src/layout/DashboardLayout.tsx
import { AppShell, Group, Title, NavLink, Stack, Text, Avatar, Menu, UnstyledButton } from '@mantine/core';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import {
  IconUsers,
  IconBriefcase,
  IconLogout,
  IconChevronDown,
} from '@tabler/icons-react';

const navItems = [
  { label: 'Recursos', icon: IconUsers, path: '/app/recursos' },
  { label: 'Servicios', icon: IconBriefcase, path: '/app/servicios' },
];

export function DashboardLayout() {
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem('turnify_token');
    localStorage.removeItem('turnify_tenant_id');
    navigate('/login', { replace: true });
  };

  return (
    <AppShell header={{ height: 64 }} navbar={{ width: 240, breakpoint: 'sm' }} padding="md">
      <AppShell.Header style={{ backgroundColor: 'white' }}>
        <Group h="100%" px="md" justify="space-between">
          <Title order={3} c="cyan.6">
            Turnify
          </Title>

          <Menu shadow="md" width={180}>
            <Menu.Target>
              <UnstyledButton>
                <Group gap="xs">
                  <Avatar color="cyan" radius="xl" size="sm">
                    M
                  </Avatar>
                  <Text size="sm" fw={500}>
                    Mi negocio
                  </Text>
                  <IconChevronDown size={14} />
                </Group>
              </UnstyledButton>
            </Menu.Target>

            <Menu.Dropdown>
              <Menu.Item leftSection={<IconLogout size={14} />} onClick={handleLogout} color="red">
                Cerrar sesión
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="sm">
        <Stack gap={4}>
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              label={item.label}
              leftSection={<item.icon size={18} />}
              active={location.pathname.startsWith(item.path)}
              onClick={() => navigate(item.path)}
              color="cyan"
              variant="light"
            />
          ))}
        </Stack>
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
}