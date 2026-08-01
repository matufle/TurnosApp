// src/lib/analytics.ts
import { MIXPANEL_TOKEN, HOTJAR_ID } from '../config/runtimeConfig';

// Sin token/id configurado, todas las funciones de acá son no-ops silenciosos —
// mismo criterio que Sentry con Sentry:Dsn vacío.

// mixpanel-browser pesa varios cientos de KB — se carga con import() dinámico recién
// cuando hace falta, para no inflar el bundle principal que paga hasta un visitante
// anónimo de la Landing (mismo criterio que el lazy() de las páginas en App.tsx).
let mixpanelClient: Promise<typeof import('mixpanel-browser').default> | null = null;

function getMixpanel() {
  if (!MIXPANEL_TOKEN) return null;
  if (!mixpanelClient) {
    mixpanelClient = import('mixpanel-browser').then(({ default: mixpanel }) => {
      mixpanel.init(MIXPANEL_TOKEN, { track_pageview: false, persistence: 'localStorage' });
      return mixpanel;
    });
  }
  return mixpanelClient;
}

export function initMixpanel() {
  getMixpanel();
}

// Alcance reducido a propósito (ver memoria del issue #67): solo estos eventos clave,
// no el funnel completo — instrumentar más recién vale la pena con volumen real de usuarios.
export async function trackEvent(name: string, props?: Record<string, unknown>) {
  const mixpanel = await getMixpanel();
  mixpanel?.track(name, props);
}

export async function identifyAnalyticsUser(id: string, props?: Record<string, unknown>) {
  const mixpanel = await getMixpanel();
  if (!mixpanel) return;
  mixpanel.identify(id);
  if (props) mixpanel.people.set(props);
}

export async function resetAnalyticsUser() {
  const mixpanel = await getMixpanel();
  mixpanel?.reset();
}

// Tag de Contentsquare (dueño de Hotjar — la cuenta quedó unificada en su plataforma,
// no en el script clásico de static.hotjar.com). Cubre heatmaps/session replay solo con
// este script; no hace falta instrumentar eventos a mano como con Mixpanel.
export function initHotjar() {
  if (!HOTJAR_ID) return;

  const script = document.createElement('script');
  script.src = `https://t.contentsquare.net/uxa/${HOTJAR_ID}.js`;
  script.defer = true;
  document.head.appendChild(script);
}
