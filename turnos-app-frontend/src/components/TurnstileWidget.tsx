// src/components/TurnstileWidget.tsx
// Widget de Cloudflare Turnstile para los formularios de registro (staff + cliente).
// La site key no es secreta (está pensada para vivir en el HTML del cliente), por eso
// tiene un fallback hardcodeado además del env var.
import { useEffect, useId, useRef } from 'react';

const SITE_KEY = import.meta.env.VITE_TURNSTILE_SITE_KEY || '0x4AAAAAAECOdabVhwy1_Ml8';
const SCRIPT_SRC = 'https://challenges.cloudflare.com/turnstile/v0/api.js';

type TurnstileRenderOptions = {
  sitekey: string;
  callback: (token: string) => void;
  'expired-callback'?: () => void;
  'error-callback'?: () => void;
};

declare global {
  interface Window {
    turnstile?: {
      render: (container: string | HTMLElement, options: TurnstileRenderOptions) => string;
      remove: (widgetId: string) => void;
      reset: (widgetId: string) => void;
    };
  }
}

let scriptPromise: Promise<void> | null = null;

function cargarScript(): Promise<void> {
  if (window.turnstile) return Promise.resolve();

  if (!scriptPromise) {
    scriptPromise = new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = SCRIPT_SRC;
      script.async = true;
      script.defer = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('No se pudo cargar Turnstile'));
      document.head.appendChild(script);
    });
  }

  return scriptPromise;
}

interface TurnstileWidgetProps {
  onVerify: (token: string) => void;
  onExpire?: () => void;
}

export function TurnstileWidget({ onVerify, onExpire }: TurnstileWidgetProps) {
  const containerId = `turnstile-${useId().replace(/[^a-zA-Z0-9]/g, '')}`;
  const widgetIdRef = useRef<string | null>(null);

  useEffect(() => {
    let cancelado = false;

    cargarScript()
      .then(() => {
        if (cancelado || !window.turnstile) return;

        widgetIdRef.current = window.turnstile.render(`#${containerId}`, {
          sitekey: SITE_KEY,
          callback: onVerify,
          'expired-callback': () => onExpire?.(),
        });
      })
      .catch(() => {
        // Si falla la carga del script (bloqueado por un adblocker, sin conexión, etc.)
        // el botón de submit queda deshabilitado porque nunca llega el token — comportamiento
        // esperado: preferimos bloquear el registro antes que dejarlo pasar sin verificar.
      });

    return () => {
      cancelado = true;
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.remove(widgetIdRef.current);
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [containerId]);

  return <div id={containerId} />;
}
