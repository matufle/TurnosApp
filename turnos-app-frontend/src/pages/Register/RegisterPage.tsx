import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import axios from 'axios';
import { useForm, isEmail, isNotEmpty, hasLength, matchesField } from '@mantine/form';
import { authService } from '../../api/authService';

export function RegisterPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const emailPrecargado = (location.state as { email?: string } | null)?.email ?? '';
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [mostrarPassword, setMostrarPassword] = useState(false);
  const [mostrarConfirmacion, setMostrarConfirmacion] = useState(false);

  const form = useForm({
    initialValues: {
      nombreNegocio: '',
      email: emailPrecargado,
      password: '',
      confirmarPassword: '',
      aceptaTerminos: false,
    },
    validate: {
      nombreNegocio: isNotEmpty('Ingresá el nombre de tu negocio'),
      email: isEmail('Ingresá un email válido'),
      password: hasLength({ min: 6 }, 'La contraseña debe tener al menos 6 caracteres'),
      confirmarPassword: matchesField('password', 'Las contraseñas no coinciden'),
      aceptaTerminos: (value) => (value ? null : 'Tenés que aceptar los términos para continuar'),
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
        setErrorMessage('Ya existe una cuenta con ese email.');
      } else {
        setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo en unos segundos.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-surface font-body-lg text-on-background min-h-screen flex flex-col">
      <main className="flex-grow flex items-center justify-center px-margin-mobile md:px-margin-desktop py-12">
        <div className="w-full max-w-[500px]">
          <div className="text-center mb-10">
            <h1 className="font-headline-lg text-headline-lg text-primary tracking-tight mb-2">Turnify</h1>
            <p className="font-title-md text-title-md text-secondary">Organizado, práctico y moderno.</p>
          </div>

          <div className="bg-surface-container-lowest rounded-[32px] p-8 md:p-12 soft-elevation border border-surface-variant/30">
            <div className="mb-8">
              <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">Creá tu cuenta</h2>
              <p className="font-body-sm text-body-sm text-on-surface-variant">
                Sumate a los negocios que ya gestionan sus turnos con claridad.
              </p>
            </div>

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

              {/* Nombre del negocio */}
              <div className="space-y-2">
                <label
                  htmlFor="nombreNegocio"
                  className="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider block"
                >
                  Nombre del negocio
                </label>
                <div className="relative">
                  <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">
                    storefront
                  </span>
                  <input
                    id="nombreNegocio"
                    type="text"
                    placeholder="Clínica del Sur"
                    className="w-full bg-surface-bright border border-outline-variant rounded-xl py-3 pl-12 pr-4 font-body-lg text-body-lg text-on-surface placeholder:text-outline/50 transition-all focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary-container/10"
                    {...form.getInputProps('nombreNegocio')}
                  />
                </div>
                {form.errors.nombreNegocio && (
                  <p className="font-body-sm text-body-sm text-error">{form.errors.nombreNegocio}</p>
                )}
              </div>

              {/* Email */}
              <div className="space-y-2">
                <label
                  htmlFor="email"
                  className="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider block"
                >
                  Email
                </label>
                <div className="relative">
                  <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">
                    mail
                  </span>
                  <input
                    id="email"
                    type="email"
                    placeholder="tu@negocio.com"
                    className="w-full bg-surface-bright border border-outline-variant rounded-xl py-3 pl-12 pr-4 font-body-lg text-body-lg text-on-surface placeholder:text-outline/50 transition-all focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary-container/10"
                    {...form.getInputProps('email')}
                  />
                </div>
                {form.errors.email && (
                  <p className="font-body-sm text-body-sm text-error">{form.errors.email}</p>
                )}
              </div>

              {/* Contraseña + Confirmación */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label
                    htmlFor="password"
                    className="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider block"
                  >
                    Contraseña
                  </label>
                  <div className="relative">
                    <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">
                      lock
                    </span>
                    <input
                      id="password"
                      type={mostrarPassword ? 'text' : 'password'}
                      placeholder="••••••••"
                      className="w-full bg-surface-bright border border-outline-variant rounded-xl py-3 pl-12 pr-12 font-body-lg text-body-lg text-on-surface placeholder:text-outline/50 transition-all focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary-container/10"
                      {...form.getInputProps('password')}
                    />
                    <button
                      type="button"
                      onClick={() => setMostrarPassword((v) => !v)}
                      aria-label={mostrarPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-secondary focus:outline-none"
                    >
                      <span className="material-symbols-outlined text-[20px]">
                        {mostrarPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                  {form.errors.password && (
                    <p className="font-body-sm text-body-sm text-error">{form.errors.password}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <label
                    htmlFor="confirmarPassword"
                    className="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider block"
                  >
                    Confirmar
                  </label>
                  <div className="relative">
                    <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">
                      lock_reset
                    </span>
                    <input
                      id="confirmarPassword"
                      type={mostrarConfirmacion ? 'text' : 'password'}
                      placeholder="••••••••"
                      className="w-full bg-surface-bright border border-outline-variant rounded-xl py-3 pl-12 pr-12 font-body-lg text-body-lg text-on-surface placeholder:text-outline/50 transition-all focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary-container/10"
                      {...form.getInputProps('confirmarPassword')}
                    />
                    <button
                      type="button"
                      onClick={() => setMostrarConfirmacion((v) => !v)}
                      aria-label={mostrarConfirmacion ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-secondary focus:outline-none"
                    >
                      <span className="material-symbols-outlined text-[20px]">
                        {mostrarConfirmacion ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                  {form.errors.confirmarPassword && (
                    <p className="font-body-sm text-body-sm text-error">{form.errors.confirmarPassword}</p>
                  )}
                </div>
              </div>

              {/* Términos y condiciones */}
              <div className="space-y-1">
                <div className="flex items-start gap-3 py-2">
                  <div className="flex items-center h-5">
                    <input
                      id="aceptaTerminos"
                      type="checkbox"
                      className="w-5 h-5 rounded border-outline-variant text-primary-container focus:ring-primary-container cursor-pointer transition-colors"
                      {...form.getInputProps('aceptaTerminos', { type: 'checkbox' })}
                    />
                  </div>
                  <label
                    htmlFor="aceptaTerminos"
                    className="font-body-sm text-body-sm text-on-surface-variant leading-tight cursor-pointer"
                  >
                    Acepto los{' '}
                    <a
                      href="#"
                      onClick={(e) => e.preventDefault()}
                      title="Próximamente"
                      className="text-primary font-semibold hover:underline"
                    >
                      Términos de Servicio
                    </a>{' '}
                    y la{' '}
                    <a
                      href="#"
                      onClick={(e) => e.preventDefault()}
                      title="Próximamente"
                      className="text-primary font-semibold hover:underline"
                    >
                      Política de Privacidad
                    </a>
                    .
                  </label>
                </div>
                {form.errors.aceptaTerminos && (
                  <p className="font-body-sm text-body-sm text-error">{form.errors.aceptaTerminos}</p>
                )}
              </div>

              {/* Acción principal */}
              <button
                type="submit"
                disabled={loading}
                className="w-full bg-primary-container hover:bg-primary disabled:opacity-70 disabled:cursor-not-allowed text-on-primary font-title-md text-title-md py-4 rounded-xl soft-elevation transition-all duration-300 transform hover:-translate-y-0.5 active:scale-95 flex items-center justify-center gap-2"
              >
                {loading ? (
                  <>
                    <span className="material-symbols-outlined animate-spin">sync</span>
                    Creando cuenta...
                  </>
                ) : (
                  'Crear cuenta'
                )}
              </button>
            </form>

            <div className="mt-8 text-center">
              <p className="font-body-sm text-body-sm text-on-surface-variant">
                ¿Ya tenés una cuenta?{' '}
                <Link to="/login" className="text-primary font-bold hover:underline underline-offset-4 ml-1">
                  Iniciar sesión
                </Link>
              </p>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
