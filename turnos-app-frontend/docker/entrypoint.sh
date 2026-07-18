#!/bin/sh
# docker/entrypoint.sh

# Genera env-config.js con las variables de entorno reales del contenedor,
# en el momento del arranque (no del build).
cat <<EOF > /usr/share/nginx/html/env-config.js
window._env_ = {
  VITE_API_BASE_URL: "${VITE_API_BASE_URL}"
};
EOF

# Arranca Nginx normalmente
exec nginx -g 'daemon off;'