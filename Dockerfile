# ============================================================
#  Dockerfile — Relojes Lamur API (ASP.NET Core 10 Preview)
#  Imagen multi-stage: build + runtime mínimo
#  Puerto: 8080
# ============================================================

# ?? Etapa 1: Build ???????????????????????????????????????????
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["webAPIAngular/webAPIAngular.csproj", "webAPIAngular/"]
RUN dotnet restore "webAPIAngular/webAPIAngular.csproj"

COPY . .
WORKDIR /src/webAPIAngular
RUN dotnet publish "webAPIAngular.csproj" -c Release -o /app/publish --no-restore

# ?? Etapa 2: Runtime ?????????????????????????????????????????
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "webAPIAngular.dll"]
