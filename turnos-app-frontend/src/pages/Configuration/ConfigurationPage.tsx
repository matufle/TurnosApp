// src/pages/Configuracion/ConfiguracionPage.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Button,
  Stack,
  Alert,
  Loader,
  Center,
  Paper,
  Text,
  Switch,
  ColorInput,
  Divider,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { IconAlertCircle, IconDeviceFloppy } from '@tabler/icons-react';
import { tenantService } from '../../api/tenantService';
import { useTenantTheme } from '../../context/useTenantTheme';   // 👈 nuevo

export function ConfigurationPage() {
  const { setColorHex } = useTenantTheme();   // 👈 nuevo

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const form = useForm({
    initialValues: {
      colorPrimario: '#0EA5E9',
      permiteReservasPublicas: false,
    },
  });

  useEffect(() => {
    let activo = true;

    const cargarConfiguracion = async () => {
      try {
        const data = await tenantService.getConfig();
        if (activo) {
          form.setValues({
            colorPrimario: data.colorPrimario || '#0EA5E9',
            permiteReservasPublicas: data.permiteReservasPublicas || false,
          });
          setErrorMessage(null);
        }
      } catch {
        if (activo) {
          setErrorMessage('No pudimos cargar la configuración actual.');
        }
      } finally {
        if (activo) {
          setLoading(false);
        }
      }
    };

    cargarConfiguracion();

    return () => {
      activo = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      await tenantService.updateConfig(values);
      setSuccessMessage('¡Configuración guardada con éxito!');

      // Actualiza el theme en caliente: dispara el re-render de ThemedApp
      // en App.tsx sin necesidad de F5. El backend ya confirmó el guardado,
      // así que este es el único lugar donde hace falta tocar el context.
      setColorHex(values.colorPrimario);
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('Hubo un problema al guardar los cambios.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack gap="lg" maw={600}>
      <Title order={2}>Configuración del Sistema</Title>

      {errorMessage && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" variant="light">
          {errorMessage}
        </Alert>
      )}

      {successMessage && (
        <Alert color="green" variant="light">
          {successMessage}
        </Alert>
      )}

      {loading ? (
        <Center py="xl">
          <Loader color="cyan" />
        </Center>
      ) : (
        <Paper withBorder radius="md" p="xl">
          <form onSubmit={form.onSubmit(handleSubmit)}>
            <Stack gap="xl">
              <div>
                <Title order={4} mb="xs">Identidad Visual</Title>
                <Text size="sm" c="dimmed" mb="md">
                  Personalizá el color principal que verán tus clientes al momento de reservar.
                </Text>
                <ColorInput
                  label="Color Primario"
                  format="hex"
                  swatches={['#0EA5E9', '#12b886', '#fab005', '#fd7e14', '#fa5252', '#be4bdb', '#7950f2']}
                  {...form.getInputProps('colorPrimario')}
                />
              </div>

              <Divider />

              <div>
                <Title order={4} mb="xs">Reglas de Negocio</Title>
                <Text size="sm" c="dimmed" mb="md">
                  Ajustá cómo funciona la agenda y quién puede interactuar con ella.
                </Text>
                <Switch
                  label="Permitir Reservas Públicas"
                  description="Si está activo, los clientes podrán autogestionar sus turnos desde un enlace público."
                  size="md"
                  color="cyan"
                  {...form.getInputProps('permiteReservasPublicas', { type: 'checkbox' })}
                />
              </div>

              <Button
                type="submit"
                color="cyan"
                loading={submitting}
                leftSection={<IconDeviceFloppy size={18} />}
                mt="md"
              >
                Guardar Cambios
              </Button>
            </Stack>
          </form>
        </Paper>
      )}
    </Stack>
  );
}