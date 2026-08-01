# Dockerfile — raíz del proyecto

# ---- Etapa 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos los .csproj usando comodines (*.csproj). 
# Así Docker los encuentra sin importar si se llaman "TurnosApp.Presentation.WebAPI.csproj" o no.
COPY ["src/Presentation.WebAPI/*.csproj", "src/Presentation.WebAPI/"]
COPY ["src/Core.Application/*.csproj", "src/Core.Application/"]
COPY ["src/Core.Domain/*.csproj", "src/Core.Domain/"]
COPY ["src/Infra.Data/*.csproj", "src/Infra.Data/"]

# Nos movemos a la carpeta de la API y restauramos desde ahí (encuentra el .csproj automáticamente)
WORKDIR /src/src/Presentation.WebAPI
RUN dotnet restore

# Volvemos a la raíz del contenedor para copiar el resto del código
WORKDIR /src
COPY . .

# Volvemos a la carpeta de la API para buildear
WORKDIR /src/src/Presentation.WebAPI
RUN dotnet build -c Release -o /app/build

# ---- Etapa 2: Publish ----
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---- Etapa 3: Runtime (imagen final, liviana) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# Corre como el usuario no-root que ya trae la imagen base (evita correr el proceso como root).
USER $APP_UID

# OJO AQUÍ: Por los logs de errores que me pasaste antes, tu aplicación genera esta DLL.
# Si al correrlo te dice "Did you mean to run dotnet SDK commands?", cambialo por "Presentation.WebAPI.dll"
ENTRYPOINT ["dotnet", "TurnosApp.Presentation.WebAPI.dll"]