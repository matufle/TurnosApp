// src/pages/Auth/LoginPage.tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Paper,
  Title,
  TextInput,
  PasswordInput,
  Button,
  Stack,
  Text,
  Alert,
} from '@mantine/core';
import { useForm, isEmail, isNotEmpty } from '@mantine/form';
import { IconAlertCircle } from '@tabler/icons-react';
import { authService } from '../../api/authService';
import axios from 'axios';

export function LoginPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm({
    initialValues: { email: '', password: '' },
    validate: {
      email: isEmail('Ingresá un email válido'),
      password: isNotEmpty('La contraseña es obligatoria'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setErrorMessage(null);
    setLoading(true);

 try {
    const response = await authService.login(values);

    localStorage.setItem('turnify_token', response.token);
    localStorage.setItem('turnify_tenant_id', response.tenantId.toString());

    navigate('/app', { replace: true });
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 409) {
      // Recordá: usamos 409 para credenciales inválidas, no 401 (ver AuthAppService)
      setErrorMessage('Email o contraseña incorrectos. Intentá de nuevo.');
    } else {
      setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo en unos segundos.');
    }
  } finally {
    setLoading(false);
  }
};

  return (
    <Container size={420} py={80}>
      <Title order={2} ta="center" fw={800}>
        Iniciar sesión
      </Title>
      <Text c="dimmed" size="sm" ta="center" mt="xs">
        Accedé al panel de gestión de tu negocio
      </Text>

      <Paper withBorder shadow="sm" p="xl" mt="xl" radius="md">
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            {errorMessage && (
              <Alert icon={<IconAlertCircle size={16} />} color="red" variant="light">
                {errorMessage}
              </Alert>
            )}

            <TextInput
              label="Email"
              placeholder="tu@negocio.com"
              required
              {...form.getInputProps('email')}
            />

            <PasswordInput
              label="Contraseña"
              placeholder="Tu contraseña"
              required
              {...form.getInputProps('password')}
            />

            <Button type="submit" color="cyan" fullWidth mt="sm" loading={loading}>
              Ingresar
            </Button>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
}