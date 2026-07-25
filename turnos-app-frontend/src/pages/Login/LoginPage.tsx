// src/pages/Auth/LoginPage.tsx
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm, isEmail, isNotEmpty } from '@mantine/form';
import { authService } from '../../api/authService';
import { useAuth } from '../../context/useAuth';
import axios from 'axios';

export function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [mostrarPassword, setMostrarPassword] = useState(false);

  const form = useForm({
    initialValues: { email: '', password: '', recordarme: false },
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

      await login(response.token, response.tenantId);

      navigate('/app', { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        setErrorMessage('Email o contraseña incorrectos. Intentá de nuevo.');
      } else {
        setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo en unos segundos.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-background">
      <main className="w-full max-w-[1100px] bg-white rounded-xl overflow-hidden flex flex-col md:flex-row soft-elevation min-h-[600px]">
        {/* Lado de branding (split screen) */}
        <div className="hidden md:flex w-1/2 bg-surface-container-low relative overflow-hidden flex-col justify-between p-12">
          <div className="relative z-10">
            <h1 className="font-headline-lg text-headline-lg text-primary tracking-tight mb-4">
              Turnify
            </h1>
            <p className="font-title-md text-title-md text-secondary max-w-sm">
              Gestión organizada, práctica y moderna para negocios que quieren crecer.
            </p>
          </div>

          <div className="relative z-10">
            <div className="flex items-center gap-4 mb-8">
              <div className="w-12 h-12 bg-primary-container rounded-lg flex items-center justify-center text-white shrink-0">
                <span className="material-symbols-outlined">event_available</span>
              </div>
              <div>
                <p className="font-title-md text-title-md text-on-surface">Turnos sin esfuerzo</p>
                <p className="font-body-sm text-body-sm text-secondary">
                  Enfocate en tu negocio, nosotros nos ocupamos de la agenda.
                </p>
              </div>
            </div>

            <div className="relative rounded-xl overflow-hidden h-48 soft-elevation border border-white bg-gradient-to-br from-primary-container to-primary flex items-center justify-center">
              <span className="material-symbols-outlined text-white/90 text-6xl">calendar_month</span>
            </div>
          </div>
        </div>

        {/* Lado del formulario */}
        <div className="w-full md:w-1/2 p-8 md:p-16 flex flex-col justify-center">
          <div className="md:hidden mb-12 flex flex-col items-center">
            <h1 className="font-headline-lg-mobile text-headline-lg-mobile text-primary tracking-tight">
              Turnify
            </h1>
            <p className="font-body-sm text-body-sm text-secondary">Simple. Moderno. Organizado.</p>
          </div>

          <header className="mb-10 text-center md:text-left">
            <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">
              Bienvenido de nuevo
            </h2>
            <p className="font-body-lg text-body-lg text-secondary">
              Iniciá sesión para acceder a tu panel.
            </p>
          </header>

          <form className="space-y-6" onSubmit={form.onSubmit(handleSubmit)} noValidate>
            {errorMessage && (
              <div
                role="alert"
                className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
              >
                <span className="material-symbols-outlined text-[20px]">error</span>
                {errorMessage}
              </div>
            )}

            {/* Email */}
            <div className="space-y-2">
              <label
                htmlFor="email"
                className="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider block"
              >
                Email
              </label>
              <div className="relative group">
                <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline group-focus-within:text-primary transition-colors">
                  mail
                </span>
                <input
                  id="email"
                  type="email"
                  placeholder="tu@negocio.com"
                  className="w-full pl-12 pr-4 py-3 bg-surface border border-outline-variant rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-container/20 focus:border-primary transition-all font-body-lg text-body-lg text-on-surface"
                  {...form.getInputProps('email')}
                />
              </div>
              {form.errors.email && (
                <p className="font-body-sm text-body-sm text-error">{form.errors.email}</p>
              )}
            </div>

            {/* Contraseña */}
            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <label
                  htmlFor="password"
                  className="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider"
                >
                  Contraseña
                </label>
                <a
                  href="#"
                  onClick={(e) => e.preventDefault()}
                  title="Próximamente"
                  className="font-body-sm text-body-sm text-primary hover:text-on-primary-fixed-variant transition-colors"
                >
                  ¿Olvidaste tu contraseña?
                </a>
              </div>
              <div className="relative group">
                <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline group-focus-within:text-primary transition-colors">
                  lock
                </span>
                <input
                  id="password"
                  type={mostrarPassword ? 'text' : 'password'}
                  placeholder="••••••••"
                  className="w-full pl-12 pr-12 py-3 bg-surface border border-outline-variant rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-container/20 focus:border-primary transition-all font-body-lg text-body-lg text-on-surface"
                  {...form.getInputProps('password')}
                />
                <button
                  type="button"
                  onClick={() => setMostrarPassword((v) => !v)}
                  aria-label={mostrarPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-secondary focus:outline-none"
                >
                  <span className="material-symbols-outlined">
                    {mostrarPassword ? 'visibility_off' : 'visibility'}
                  </span>
                </button>
              </div>
              {form.errors.password && (
                <p className="font-body-sm text-body-sm text-error">{form.errors.password}</p>
              )}
            </div>

            {/* Recordarme */}
            <div className="flex items-center">
              <input
                id="recordarme"
                type="checkbox"
                className="w-5 h-5 rounded border-outline-variant text-primary focus:ring-primary-container"
                {...form.getInputProps('recordarme', { type: 'checkbox' })}
              />
              <label htmlFor="recordarme" className="ml-3 font-body-sm text-body-sm text-secondary select-none">
                Recordarme por 30 días
              </label>
            </div>

            {/* Botón de acción */}
            <button
              type="submit"
              disabled={loading}
              className="w-full bg-primary-container hover:bg-primary disabled:opacity-70 disabled:cursor-not-allowed text-white font-title-md text-title-md py-4 rounded-lg soft-elevation transition-all duration-300 transform hover:-translate-y-0.5 active:scale-95 flex items-center justify-center gap-2"
            >
              {loading ? (
                <>
                  <span className="material-symbols-outlined animate-spin">sync</span>
                  Ingresando...
                </>
              ) : (
                <>
                  Ingresar
                  <span className="material-symbols-outlined">arrow_forward</span>
                </>
              )}
            </button>
          </form>

          <footer className="mt-10 text-center">
            <p className="font-body-lg text-body-lg text-secondary">
              ¿No tenés una cuenta?{' '}
              <Link to="/registro" className="text-primary font-title-md hover:underline underline-offset-4">
                Unite a Turnify
              </Link>
            </p>
          </footer>
        </div>
      </main>
    </div>
  );
}
