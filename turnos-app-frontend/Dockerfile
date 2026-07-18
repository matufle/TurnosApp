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

EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]