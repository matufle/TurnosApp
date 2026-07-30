// src/components/OnboardingTour/OnboardingTour.tsx
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Joyride, STATUS, type Step, type EventData, type ButtonType } from 'react-joyride';
import { useAuth } from '../../context/useAuth';
import { navLinks, esLinkVisible } from '../../layout/navLinks';

interface PaginaTour {
  path: string;
  steps: Step[];
}

// Mismo orden que el sidebar (navLinks en DashboardLayout). Cada página aporta 1-2 steps
// resaltando lo esencial para arrancar a operar; el primer step de cada página apunta al
// link del sidebar (siempre presente, sin importar en qué /app/* estemos parados).
const PAGINAS_TOUR: PaginaTour[] = [
  {
    path: '/app/turnos',
    steps: [
      {
        target: '[data-tour="nav-turnos"]',
        title: '¡Bienvenido a Turnify!',
        content: 'Te vamos a mostrar rápido las secciones principales. Empecemos por Turnos: acá vas a ver y agendar la agenda de tu negocio.',
        placement: 'right',
      },
      {
        target: '[data-tour="turnos-calendario"]',
        content: 'Hacé click en un horario libre del calendario, o usá el botón "Nuevo turno", para agendar el primero.',
      },
    ],
  },
  {
    path: '/app/clientes',
    steps: [
      {
        target: '[data-tour="nav-clientes"]',
        content: 'En Clientes administrás la cartera de personas a las que les agendás turnos.',
        placement: 'right',
      },
      {
        target: '[data-tour="clientes-nuevo"]',
        content: 'Agregá tu primer cliente para poder asignarle un turno.',
      },
    ],
  },
  {
    path: '/app/recursos',
    steps: [
      {
        target: '[data-tour="nav-recursos"]',
        content: 'Los Recursos son las personas o espacios que se reservan (profesionales, canchas, boxes, etc.).',
        placement: 'right',
      },
      {
        target: '[data-tour="recursos-nuevo"]',
        content: 'Creá tu primer recurso: sin al menos uno, todavía no vas a poder agendar turnos.',
      },
    ],
  },
  {
    path: '/app/servicios',
    steps: [
      {
        target: '[data-tour="nav-servicios"]',
        content: 'En Servicios definís qué ofrecés, con su duración y precio.',
        placement: 'right',
      },
      {
        target: '[data-tour="servicios-nuevo"]',
        content: 'Creá tu primer servicio para poder ofrecerlo al agendar un turno.',
      },
    ],
  },
  {
    path: '/app/metodos-pago',
    steps: [
      {
        target: '[data-tour="nav-metodos-pago"]',
        content: 'Configurá los métodos de pago que aceptás, con recargos o descuentos si aplican.',
        placement: 'right',
      },
      {
        target: '[data-tour="metodospago-nuevo"]',
        content: 'Agregá tu primer método de pago para poder registrar cobros.',
      },
    ],
  },
  {
    path: '/app/cobros',
    steps: [
      {
        target: '[data-tour="nav-cobros"]',
        content: 'Acá vas a ver el historial completo de cobros registrados en tus turnos.',
        placement: 'right',
      },
      {
        target: '[data-tour="cobros-filtros"]',
        content: 'Filtrá por cliente o por rango de fechas para encontrar un cobro puntual.',
      },
    ],
  },
  {
    path: '/app/metricas',
    steps: [
      {
        target: '[data-tour="nav-metricas"]',
        content: 'Métricas te muestra el panorama financiero y operativo de tu negocio.',
        placement: 'right',
      },
      {
        target: '[data-tour="metricas-tabs"]',
        content: 'Recorré las pestañas: resumen, ingresos, turnos, clientes y servicios/recursos.',
      },
    ],
  },
  {
    path: '/app/usuarios',
    steps: [
      {
        target: '[data-tour="nav-usuarios"]',
        content: 'Invitá a tu equipo dándoles acceso al sistema desde Usuarios.',
        placement: 'right',
      },
      {
        target: '[data-tour="usuarios-nuevo"]',
        content: 'Creá un usuario nuevo y asignale un rol.',
      },
    ],
  },
  {
    path: '/app/roles',
    steps: [
      {
        target: '[data-tour="nav-roles"]',
        content: 'En Roles y Permisos definís a medida qué puede ver y hacer cada miembro de tu equipo.',
        placement: 'right',
      },
      {
        target: '[data-tour="roles-nuevo"]',
        content: 'Creá un rol nuevo con los permisos que necesites.',
      },
    ],
  },
  {
    path: '/app/configuracion',
    steps: [
      {
        target: '[data-tour="nav-configuracion"]',
        content: 'Por último, en Configuración personalizás la identidad visual y las reglas de negocio de tu cuenta.',
        placement: 'right',
      },
      {
        target: '[data-tour="config-color"]',
        content: 'Elegí el color principal de tu marca.',
      },
      {
        target: '[data-tour="config-solapamiento"]',
        content: '¡Y listo! Esta opción decide si permitís turnos superpuestos en el mismo horario. Ya podés empezar a usar Turnify.',
      },
    ],
  },
];

// Objeto estable (misma referencia entre renders) para que Joyride nunca reciba un `options`
// "nuevo" en cada re-render.
const JOYRIDE_OPTIONS = {
  skipBeacon: true,
  showProgress: true,
  buttons: ['back', 'close', 'primary', 'skip'] as ButtonType[],
  zIndex: 10000,
  primaryColor: '#0EA5E9',
  // Varias páginas muestran un loader mientras traen datos (recursos/servicios/turnos) antes
  // de montar el elemento anclado; el default de 1s no alcanza tras navegar entre rutas, según
  // quedó demostrado en pruebas manuales.
  targetWaitTimeout: 8000,
};

export function OnboardingTour() {
  const { user, hasPermission, completarOnboarding } = useAuth();
  const navigate = useNavigate();
  const completadoRef = useRef(false);
  const [paginaIndex, setPaginaIndex] = useState(0);

  const paginasVisibles = useMemo(() => {
    const pathsVisibles = new Set(
      navLinks.filter((link) => esLinkVisible(link.permiso, hasPermission)).map((link) => link.path)
    );
    return PAGINAS_TOUR.filter((pagina) => pathsVisibles.has(pagina.path));
  }, [hasPermission]);

  const paginaActual = paginasVisibles[paginaIndex];
  const esUltimaPagina = paginaIndex === paginasVisibles.length - 1;

  // Cada página de la gira monta una instancia NUEVA de Joyride (key={paginaIndex}), en vez de
  // mantener una sola instancia viva cruzando rutas: en pruebas manuales, tanto el modo
  // no-controlado (continuous + after/onEvent) como el controlado (stepIndex explícito) dejaban
  // de avanzar tras la primera transición entre páginas — un remontaje limpio por página evita
  // ese problema de sincronización interna por completo.
  //
  // Guardado por `.path` (no solo por deps) para no pelear con la navegación del propio usuario:
  // sin este guard, cualquier re-render que recreara `user`/`paginaActual` (referencia nueva,
  // mismo path) volvía a disparar el navigate y devolvía a la fuerza al usuario a la página del
  // paso actual del tour ante CUALQUIER click de navegación mientras el onboarding no estuviera
  // completo — incluyendo clicks normales en el sidebar, no solo un refresh de página.
  const ultimaPaginaNavegadaRef = useRef<string | null>(null);
  useEffect(() => {
    if (!user || user.onboardingCompletado || !paginaActual) return;
    if (ultimaPaginaNavegadaRef.current === paginaActual.path) return;
    ultimaPaginaNavegadaRef.current = paginaActual.path;
    if (window.location.pathname !== paginaActual.path) {
      navigate(paginaActual.path);
    }
  }, [user, paginaActual, navigate]);

  const handleEvent = useCallback(
    (data: EventData) => {
      if (data.status === STATUS.SKIPPED) {
        if (!completadoRef.current) {
          completadoRef.current = true;
          completarOnboarding().catch(() => {
            // Un fallo al persistir no debe romper el flujo del usuario: el tour ya cerró igual.
          });
        }
        return;
      }

      if (data.status === STATUS.FINISHED) {
        if (esUltimaPagina) {
          if (!completadoRef.current) {
            completadoRef.current = true;
            completarOnboarding().catch(() => {
              // Un fallo al persistir no debe romper el flujo del usuario: el tour ya cerró igual.
            });
          }
        } else {
          setPaginaIndex((i) => i + 1);
        }
      }
    },
    [esUltimaPagina, completarOnboarding]
  );

  if (!user || user.onboardingCompletado || !paginaActual) {
    return null;
  }

  return (
    <Joyride
      key={paginaIndex}
      steps={paginaActual.steps}
      run
      continuous
      onEvent={handleEvent}
      options={JOYRIDE_OPTIONS}
      locale={{
        back: 'Atrás',
        close: 'Cerrar',
        // "Finalizar" solo en el último step de la ÚLTIMA página; en el resto, aunque sea el
        // último step de ESTA instancia, todavía sigue el resto de la gira.
        last: esUltimaPagina ? 'Finalizar' : 'Siguiente',
        next: 'Siguiente',
        nextWithProgress: 'Siguiente ({current} de {total})',
        skip: 'Saltar tour',
      }}
    />
  );
}
