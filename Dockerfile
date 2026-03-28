# ============================================================
#  Dockerfile — Relojes Lamur API (ASP.NET Core 10)
#  Imagen multi-stage: build + runtime mínimo
#  Compatible con: Railway, Cloud Run, Render, Fly.io
#  Puerto: 8080 (requerido por Railway y Cloud Run)
# ============================================================

# ?? Etapa 1: Build ???????????????????????????????????????????
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar proyecto y restaurar dependencias (capa cacheada)
COPY ["webAPIAngular/webAPIAngular.csproj", "webAPIAngular/"]
RUN dotnet restore "webAPIAngular/webAPIAngular.csproj"

# Copiar todo el código y publicar en Release
COPY . .
WORKDIR /src/webAPIAngular
RUN dotnet publish "webAPIAngular.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ?? Etapa 2: Runtime mínimo ??????????????????????????????????
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Cloud Run exige que la app escuche en 0.0.0.0:8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Copiar solo el publicado
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "webAPIAngular.dll"]
