import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

export function LandingPage() {
  const navigate = useNavigate();
  const [scrolled, setScrolled] = useState(false);
  const [emailCta, setEmailCta] = useState('');

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 10);
    window.addEventListener('scroll', onScroll);
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  const handleCtaSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate('/registro', { state: { email: emailCta } });
  };

  return (
    <div className="bg-surface-bright text-on-surface font-body-lg text-body-lg min-h-screen antialiased selection:bg-primary-container selection:text-on-primary-container">
      {/* Header */}
      <header
        className={`fixed top-0 w-full z-50 transition-all duration-300 ${
          scrolled ? 'bg-surface-container-lowest shadow-[0px_4px_20px_rgba(0,188,212,0.08)]' : 'bg-transparent'
        }`}
      >
        <div className="flex justify-between items-center px-margin-mobile md:px-margin-desktop w-full max-w-container-max mx-auto h-20">
          <div className="flex items-center gap-2">
            <span
              className="material-symbols-outlined text-primary text-3xl"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              calendar_today
            </span>
            <span className="font-headline-lg-mobile text-headline-lg-mobile md:font-headline-lg md:text-headline-lg text-primary tracking-tight">
              Slotia
            </span>
          </div>

          <div className="hidden md:flex gap-8 items-center">
            <a
              href="#features"
              className="text-secondary font-body-lg text-body-lg no-underline hover:text-primary-container transition-colors duration-200"
            >
              Características
            </a>
            <a
              href="#beneficios"
              className="text-secondary font-body-lg text-body-lg no-underline hover:text-primary-container transition-colors duration-200"
            >
              Beneficios
            </a>
          </div>

          <div className="flex items-center gap-1.5 md:gap-3">
            <Link
              to="/login"
              className="whitespace-nowrap border border-primary-container text-primary-container px-2.5 py-1.5 text-xs font-semibold no-underline md:px-6 md:py-3 md:font-title-md md:text-title-md rounded-full hover:bg-primary-container hover:text-on-primary-container transition-colors duration-300"
            >
              Iniciar sesión
            </Link>
            <Link
              to="/registro"
              className="whitespace-nowrap bg-primary-container text-on-primary-container px-2.5 py-1.5 text-xs font-semibold no-underline md:px-6 md:py-3 md:font-title-md md:text-title-md rounded-full hover:bg-primary hover:text-on-primary transition-colors duration-300 shadow-[0px_4px_20px_rgba(0,188,212,0.2)]"
            >
              Comenzar Gratis
            </Link>
          </div>
        </div>
      </header>

      <main className="pt-24">
        {/* Hero */}
        <section className="relative min-h-[880px] flex items-center pt-12 pb-24 overflow-hidden">
          <div className="absolute inset-0 z-0 opacity-40 pointer-events-none">
            <div className="absolute top-[-10%] right-[-5%] w-96 h-96 bg-primary-container rounded-full mix-blend-multiply filter blur-3xl opacity-50 animate-blob" />
            <div className="absolute top-[20%] left-[-10%] w-72 h-72 bg-secondary-container rounded-full mix-blend-multiply filter blur-3xl opacity-50 animate-blob animation-delay-2000" />
          </div>

          <div className="relative z-10 w-full max-w-container-max mx-auto px-margin-mobile md:px-margin-desktop grid md:grid-cols-2 gap-gutter items-center">
            <div className="flex flex-col gap-8 md:pr-12">
              <div className="inline-flex items-center gap-2 bg-surface-container-high px-4 py-2 rounded-full w-fit">
                <span
                  className="material-symbols-outlined text-primary text-sm"
                  style={{ fontVariationSettings: "'FILL' 1" }}
                >
                  star
                </span>
                <span className="font-label-md text-label-md text-primary uppercase">
                  La nueva forma de organizar
                </span>
              </div>

              <h1 className="font-display-lg text-display-lg text-on-background leading-tight">
                Gestioná tus turnos de forma <span className="text-primary">profesional</span> y{' '}
                <span className="text-primary-container">simple</span>
              </h1>

              <p className="font-body-lg text-body-lg text-secondary md:text-lg max-w-xl">
                Olvidate de las agendas en papel y los mensajes desordenados. Slotia centraliza tus
                reservas, organiza tus recursos y servicios, y te da una vista clara de tu semana.
              </p>

              <div className="flex flex-col sm:flex-row gap-4 mt-4">
                <Link
                  to="/registro"
                  className="bg-primary-container text-on-primary-container px-8 py-4 rounded-full font-title-md text-title-md no-underline hover:bg-primary hover:text-on-primary transition-all duration-300 shadow-[0px_4px_20px_rgba(0,188,212,0.3)] hover:shadow-[0px_6px_24px_rgba(0,188,212,0.4)] flex items-center justify-center gap-2"
                >
                  Crear mi cuenta
                  <span className="material-symbols-outlined">arrow_forward</span>
                </Link>
                <a
                  href="#features"
                  className="bg-surface-container text-on-surface px-8 py-4 rounded-full font-title-md text-title-md no-underline hover:bg-surface-variant transition-colors duration-300 flex items-center justify-center gap-2 border border-outline-variant"
                >
                  <span className="material-symbols-outlined">visibility</span>
                  Ver funciones
                </a>
              </div>
            </div>

            {/* Hero illustration */}
            <div className="relative w-full h-[500px] mt-12 md:mt-0">
              <div className="absolute inset-4 bg-surface-container-lowest rounded-3xl shadow-[0px_4px_40px_rgba(0,188,212,0.12)] border border-surface-container p-6 z-10 flex flex-col gap-4 transform rotate-1 hover:rotate-0 transition-transform duration-500">
                <div className="flex justify-between items-center mb-2">
                  <div>
                    <h3 className="font-title-md text-title-md text-on-surface">Agenda Semanal</h3>
                    <p className="text-sm text-secondary">Julio 2026</p>
                  </div>
                  <div className="flex gap-2">
                    <div className="w-8 h-8 rounded-full bg-surface-container flex items-center justify-center text-secondary">
                      <span className="material-symbols-outlined text-sm">chevron_left</span>
                    </div>
                    <div className="w-8 h-8 rounded-full bg-surface-container flex items-center justify-center text-secondary">
                      <span className="material-symbols-outlined text-sm">chevron_right</span>
                    </div>
                  </div>
                </div>

                <div className="flex-1 flex gap-4 overflow-hidden relative">
                  <div className="flex flex-col gap-8 text-xs text-secondary pt-2">
                    <span>09:00</span>
                    <span>10:00</span>
                    <span>11:00</span>
                    <span>12:00</span>
                  </div>
                  <div className="flex-1 grid grid-cols-2 gap-4 relative">
                    <div className="bg-secondary-container bg-opacity-30 border-l-4 border-primary rounded-r-lg p-3 h-24 absolute top-0 left-0 w-[45%]">
                      <p className="font-bold text-sm text-on-primary-container">Consulta Inicial</p>
                      <p className="text-xs text-secondary mt-1">Ana Martínez</p>
                    </div>
                    <div className="bg-primary-container bg-opacity-20 border-l-4 border-primary-container rounded-r-lg p-3 h-32 absolute top-16 right-0 w-[45%]">
                      <p className="font-bold text-sm text-on-primary-container">Revisión de Proyecto</p>
                      <p className="text-xs text-secondary mt-1">Carlos Ruiz</p>
                    </div>
                  </div>
                </div>
              </div>

              <div
                className="absolute -left-6 top-1/4 glass-card rounded-2xl p-4 shadow-[0px_4px_20px_rgba(0,188,212,0.15)] z-20 flex items-center gap-3 animate-bounce"
                style={{ animationDuration: '3s' }}
              >
                <div className="w-10 h-10 rounded-full bg-tertiary-container flex items-center justify-center text-on-tertiary-container">
                  <span className="material-symbols-outlined">event_available</span>
                </div>
                <div>
                  <p className="font-bold text-sm">Turno confirmado</p>
                  <p className="text-xs text-secondary">Con Ana Martínez</p>
                </div>
              </div>

              <div
                className="absolute -right-4 bottom-1/4 glass-card rounded-2xl p-4 shadow-[0px_4px_20px_rgba(0,188,212,0.15)] z-20 flex items-center gap-3 animate-bounce"
                style={{ animationDuration: '4s' }}
              >
                <div className="w-10 h-10 rounded-full bg-primary-container flex items-center justify-center text-on-primary-container">
                  <span
                    className="material-symbols-outlined"
                    style={{ fontVariationSettings: "'FILL' 1" }}
                  >
                    check_circle
                  </span>
                </div>
                <div>
                  <p className="font-bold text-sm">Nuevo turno</p>
                  <p className="text-xs text-secondary">Para mañana, 10:00</p>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Features Bento Grid */}
        <section className="py-24 bg-surface-container-lowest" id="features">
          <div className="w-full max-w-container-max mx-auto px-margin-mobile md:px-margin-desktop">
            <div className="text-center max-w-2xl mx-auto mb-16">
              <h2 className="font-headline-lg-mobile text-headline-lg-mobile md:font-headline-lg md:text-headline-lg text-on-background mb-4">
                Todo lo que necesitás, nada de lo que sobra
              </h2>
              <p className="font-body-lg text-body-lg text-secondary">
                Diseñado con un enfoque minimalista para reducir la carga administrativa y maximizar
                tu eficiencia.
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 auto-rows-auto md:auto-rows-[250px]">
              {/* Item grande: Agenda visual */}
              <div className="md:col-span-2 md:row-span-2 bg-surface-bright rounded-3xl p-8 border border-surface-container relative overflow-hidden group hover:shadow-[0px_8px_30px_rgba(0,188,212,0.06)] transition-all duration-300">
                <div className="absolute top-0 right-0 w-64 h-64 bg-primary-container opacity-5 rounded-full blur-3xl transform translate-x-1/2 -translate-y-1/2" />
                <div className="relative z-10 flex flex-col h-full">
                  <div className="w-12 h-12 rounded-2xl bg-primary-container bg-opacity-10 text-primary flex items-center justify-center mb-6">
                    <span className="material-symbols-outlined text-2xl">calendar_view_week</span>
                  </div>
                  <h3 className="font-title-md text-title-md text-on-surface mb-3">
                    Agenda visual centralizada
                  </h3>
                  <p className="text-secondary mb-6 max-w-md">
                    Vista por día, semana o mes con un color distinto por cada recurso. Creá un turno
                    seleccionando el horario directo en el calendario.
                  </p>
                  <div className="mt-auto bg-surface-container-lowest rounded-xl p-4 border border-surface-container shadow-sm w-3/4 max-w-sm transform group-hover:-translate-y-2 transition-transform duration-300">
                    <div className="flex items-center gap-3 mb-3">
                      <div className="w-8 h-8 rounded-full bg-surface-variant flex items-center justify-center">
                        <span className="material-symbols-outlined text-sm text-primary">
                          calendar_month
                        </span>
                      </div>
                      <div className="h-2 w-32 bg-surface-container rounded-full" />
                    </div>
                    <div className="grid grid-cols-4 gap-2">
                      <div className="h-8 rounded bg-primary-container bg-opacity-20 flex items-center justify-center text-xs font-bold text-primary">
                        10:00
                      </div>
                      <div className="h-8 rounded bg-surface-variant opacity-50 flex items-center justify-center text-xs text-secondary line-through">
                        11:00
                      </div>
                      <div className="h-8 rounded bg-primary-container bg-opacity-20 flex items-center justify-center text-xs font-bold text-primary">
                        12:00
                      </div>
                      <div className="h-8 rounded bg-primary-container bg-opacity-20 flex items-center justify-center text-xs font-bold text-primary">
                        13:00
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Item: recursos/servicios/clientes */}
              <div className="bg-surface-bright rounded-3xl p-8 border border-surface-container relative overflow-hidden group hover:shadow-[0px_8px_30px_rgba(0,188,212,0.06)] transition-all duration-300">
                <div className="relative z-10">
                  <div className="w-12 h-12 rounded-2xl bg-tertiary-container bg-opacity-10 text-tertiary flex items-center justify-center mb-6">
                    <span className="material-symbols-outlined text-2xl">dashboard_customize</span>
                  </div>
                  <h3 className="font-title-md text-title-md text-on-surface mb-3">
                    Recursos, servicios y clientes
                  </h3>
                  <p className="text-secondary text-sm">
                    Administrá tus profesionales o salas, definí precios y duración de cada servicio, y
                    llevá el historial de tus clientes en un solo lugar.
                  </p>
                </div>
              </div>

              {/* Item: multi-tenant */}
              <div className="bg-surface-bright rounded-3xl p-8 border border-surface-container relative overflow-hidden group hover:shadow-[0px_8px_30px_rgba(0,188,212,0.06)] transition-all duration-300">
                <div className="relative z-10">
                  <div className="w-12 h-12 rounded-2xl bg-secondary-container bg-opacity-20 text-secondary flex items-center justify-center mb-6">
                    <span className="material-symbols-outlined text-2xl">storefront</span>
                  </div>
                  <h3 className="font-title-md text-title-md text-on-surface mb-3">
                    Tu negocio, tu marca
                  </h3>
                  <p className="text-secondary text-sm">
                    Cada cuenta tiene sus datos aislados y su propio color de marca en toda la
                    interfaz. Sin configuraciones complejas para empezar.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* CTA */}
        <section className="py-24 relative overflow-hidden" id="beneficios">
          <div className="absolute inset-0 bg-primary opacity-5 pointer-events-none" />
          <div className="w-full max-w-container-max mx-auto px-margin-mobile md:px-margin-desktop relative z-10">
            <div className="bg-surface-container-lowest rounded-3xl p-12 md:p-16 shadow-[0px_4px_40px_rgba(0,188,212,0.08)] border border-primary-container border-opacity-20 text-center flex flex-col items-center max-w-4xl mx-auto">
              <div className="w-16 h-16 rounded-full bg-primary-container text-on-primary-container flex items-center justify-center mb-8">
                <span className="material-symbols-outlined text-3xl">rocket_launch</span>
              </div>
              <h2 className="font-display-lg text-display-lg text-on-background mb-6">
                Empezá a organizar tu negocio hoy
              </h2>
              <p className="font-body-lg text-body-lg text-secondary mb-10 max-w-2xl">
                Centralizá tus turnos, tus recursos y tus clientes desde el primer día. Empezá gratis,
                sin tarjeta de crédito.
              </p>
              <form className="w-full max-w-md flex flex-col sm:flex-row gap-3" onSubmit={handleCtaSubmit}>
                <input
                  type="email"
                  required
                  value={emailCta}
                  onChange={(e) => setEmailCta(e.target.value)}
                  placeholder="Tu correo electrónico"
                  className="flex-1 rounded-full border border-outline-variant bg-surface-bright px-6 py-4 focus:outline-none focus:border-primary-container focus:ring-1 focus:ring-primary-container shadow-sm transition-all duration-200"
                />
                <button
                  type="submit"
                  className="bg-primary text-on-primary px-8 py-4 rounded-full font-title-md text-title-md hover:bg-on-primary-fixed-variant transition-colors duration-300 shadow-md whitespace-nowrap"
                >
                  Registrarme
                </button>
              </form>
              <p className="text-xs text-secondary mt-4">Sin tarjeta de crédito. Cancelá cuando quieras.</p>
            </div>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="w-full py-12 px-margin-mobile md:px-margin-desktop flex flex-col md:flex-row justify-between items-center gap-base max-w-container-max mx-auto bg-surface-container-low border-t border-surface-container">
        <div className="font-headline-lg text-headline-lg text-primary flex items-center gap-2">
          <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>
            calendar_today
          </span>
          Slotia
        </div>

        <nav className="flex flex-wrap justify-center gap-6">
          {[
            { label: 'Política de Privacidad', href: '/privacidad' },
            { label: 'Términos de Servicio', href: '/terminos' },
            { label: 'Centro de Ayuda', href: '#' },
            { label: 'Contacto', href: '#' },
          ].map(({ label, href }) => (
            <a
              key={label}
              href={href}
              onClick={href === '#' ? (e) => e.preventDefault() : undefined}
              title={href === '#' ? 'Próximamente' : undefined}
              className="text-on-secondary-container font-body-sm text-body-sm no-underline hover:text-primary hover:underline underline-offset-4 transition-all duration-300"
            >
              {label}
            </a>
          ))}
        </nav>

        <div className="font-body-sm text-body-sm text-secondary">© 2026 Slotia. Organizado, práctico y moderno.</div>
      </footer>
    </div>
  );
}
