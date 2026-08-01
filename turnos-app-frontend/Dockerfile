# ---- Etapa 1: Build ----
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# ---- Etapa 2: Runtime (Nginx sirviendo estáticos) ----
FROM nginx:alpine AS final
COPY docker/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

COPY docker/entrypoint.sh /docker-entrypoint.sh
# Convierte CRLF -> LF antes de dar permisos de ejecución.
# Necesario porque el archivo fue creado/editado en Windows.
RUN sed -i 's/\r$//' /docker-entrypoint.sh && chmod +x /docker-entrypoint.sh

# Corre como el usuario no-root "nginx" que ya trae la imagen (por eso escucha en
# :8080 y no en :80 — un puerto <1024 requiere root para bindearlo). Le damos
# ownership de lo que necesita tocar en runtime: cache/logs de nginx, el pid file,
# y el html donde entrypoint.sh escribe env-config.js en cada arranque.
RUN chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html \
    && touch /var/run/nginx.pid && chown nginx:nginx /var/run/nginx.pid

USER nginx
EXPOSE 8080
ENTRYPOINT ["/docker-entrypoint.sh"]