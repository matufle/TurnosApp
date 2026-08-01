import React from 'react';
import ReactDOM from 'react-dom/client';
import * as Sentry from '@sentry/react';
import '@mantine/core/styles.css';
import '@mantine/charts/styles.css';
import './tailwind.css';
import App from './App';
import { SENTRY_DSN } from './config/runtimeConfig';
import { initMixpanel, initHotjar } from './lib/analytics';

// DSN vacío (sin configurar) desactiva el SDK sin romper nada.
if (SENTRY_DSN) {
  Sentry.init({
    dsn: SENTRY_DSN,
    environment: import.meta.env.MODE,
    tracesSampleRate: 0.2,
  });
}

initMixpanel();
initHotjar();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Sentry.ErrorBoundary fallback={<p>Ocurrió un error inesperado. Recargá la página.</p>}>
      <App />
    </Sentry.ErrorBoundary>
  </React.StrictMode>
);