// src/components/EmptyState.tsx
import { Stack, Text, Button, ThemeIcon, Center } from '@mantine/core';

interface EmptyStateProps {
  title: string;
  description: string;
  icon: React.ElementType; // Recibe un ícono de Tabler
  actionLabel: string;
  onAction: () => void;
}

export function EmptyState({ title, description, icon: Icon, actionLabel, onAction }: EmptyStateProps) {
  return (
    <Center py={80}>
      <Stack align="center" gap="sm">
        {/* El ThemeIcon va a heredar tu color primario automáticamente */}
        <ThemeIcon size={80} radius="100%" variant="light">
          <Icon size={40} stroke={1.5} />
        </ThemeIcon>
        
        <Text fw={600} size="xl" mt="md">
          {title}
        </Text>
        
        <Text c="dimmed" size="sm" ta="center" maw={350}>
          {description}
        </Text>
        
        <Button mt="lg" onClick={onAction} size="md">
          {actionLabel}
        </Button>
      </Stack>
    </Center>
  );
}