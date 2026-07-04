import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
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
import { useForm, isEmail, isNotEmpty, hasLength } from '@mantine/form';
import { IconAlertCircle } from '@tabler/icons-react';
import { authService } from '../../api/authService';

export function RegisterPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm({
    initialValues: { nombreNegocio: '', email: '', password: '' },
    validate: {
      nombreNegocio: isNotEmpty('Ingresá el nombre de tu negocio'),
      email: isEmail('Ingresá un email válido'),
      password: hasLength({ min: 6 }, 'La contraseña debe tener al menos 6 caracteres'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setErrorMessage(null);
    setLoading(true);

    try {
      const response = await authService.register(values);

      localStorage.setItem('turnify_token', response.token);
      localStorage.setItem('turnify_tenant_id', response.tenantId.toString());

      navigate('/app', { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        // Mismo criterio que en Login: 409 lo usamos para conflictos de negocio
        // (acá, email ya registrado — ver AuthAppService.RegisterAsync)
        setErrorMessage('Ya existe una cuenta con ese email.');
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
        Registrá tu negocio
      </Title>
      <Text c="dimmed" size="sm" ta="center" mt="xs">
        Empezá a gestionar tus turnos en minutos
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
              label="Nombre del negocio"
              placeholder="Clínica del Sur"
              required
              {...form.getInputProps('nombreNegocio')}
            />

            <TextInput
              label="Email"
              placeholder="tu@negocio.com"
              required
              {...form.getInputProps('email')}
            />

            <PasswordInput
              label="Contraseña"
              placeholder="Mínimo 6 caracteres"
              required
              {...form.getInputProps('password')}
            />

            <Button type="submit" color="cyan" fullWidth mt="sm" loading={loading}>
              Crear cuenta
            </Button>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
}