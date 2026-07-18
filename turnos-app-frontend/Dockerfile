# ---- Etapa 1: Build ----
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# ---- Etapa 2: Runtime (Nginx sirviendo estáticos) ----
FROM nginx:alpine AS final

# Reemplaza la config default de Nginx por la nuestra (SPA-aware)
COPY docker/default.conf /etc/nginx/conf.d/default.conf

# Copiamos el build ya compilado desde la etapa anterior
COPY --from=build /app/dist /usr/share/nginx/html

# Script de entrypoint que genera env-config.js al arrancar
COPY docker/entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]