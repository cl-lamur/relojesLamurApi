# ?? Comandos — Relojes Lamur API
> Referencia rápida para desarrollo local, migraciones y despliegue a producción en Render.com

---

## ?? Requisitos previos
| Herramienta | Versión |
|-------------|---------|
| .NET SDK | 10.0 Preview |
| Docker Desktop | Cualquier versión reciente |
| Git | Cualquier versión |

---

## ?? Despliegue a producción (Render.com)

### Flujo completo
```bash
# 1. Asegurarse de estar en la rama master
git checkout master

# 2. Ver qué archivos cambiaron
git status

# 3. Agregar todos los cambios
git add .

# 4. Hacer commit con mensaje descriptivo
git commit -m "feat: descripción del cambio"

# 5. Subir a GitHub ? Render despliega automáticamente
git push origin master
```

> ? Render detecta el push y construye la imagen Docker automáticamente.  
> ? El build tarda entre **5 y 10 minutos** la primera vez.

---

### Verificar que el deploy fue exitoso
```
https://relojeslamurapi.onrender.com/health     ? debe responder: {"status":"healthy"}
https://relojeslamurapi.onrender.com            ? debe mostrar el Swagger UI
```

---

## ?? Desarrollo local

### Ejecutar la API localmente
```bash
cd webAPIAngular
dotnet run
```

### Ejecutar con recarga automática al guardar
```bash
cd webAPIAngular
dotnet watch run
```

### Compilar sin ejecutar
```bash
cd webAPIAngular
dotnet build
```

---

## ??? Migraciones de base de datos (Entity Framework)

### Crear una nueva migración
```bash
cd webAPIAngular
dotnet ef migrations add NombreDeLaMigracion
```

### Aplicar migraciones pendientes a la base de datos
```bash
cd webAPIAngular
dotnet ef database update
```

### Ver lista de migraciones existentes
```bash
cd webAPIAngular
dotnet ef migrations list
```

### Revertir la última migración
```bash
cd webAPIAngular
dotnet ef migrations remove
```

---

## ?? Seed y reset de base de datos

### Reset completo + re-seed (?? borra todos los datos)
```bash
cd webAPIAngular
dotnet run -- --reset
```

---

## ?? Docker (prueba local antes de subir a Render)

### Construir la imagen Docker localmente
```bash
# Ejecutar desde la raíz del repositorio (donde está el Dockerfile)
docker build -t relojes-lamur-api .
```

### Correr el contenedor localmente
```bash
docker run -p 8080:8080 `
  -e "ConnectionStrings__DefaultConnection=Server=gondola.proxy.rlwy.net;Port=26213;Database=railway;User=root;Password=aCtzoIjPzXdTDZqEFgWOWIDgUgdLPEMe;SslMode=Required;AllowPublicKeyRetrieval=true;CharSet=utf8mb4;" `
  -e "Jwt__Secret=RelojesLamur_SuperSecretKey_MinLength32chars!" `
  -e "Jwt__Issuer=RelojesLamurAPI" `
  -e "Jwt__Audience=RelojesLamurClient" `
  -e "Cors__AllowedOrigins__0=http://localhost:4200" `
  relojes-lamur-api
```

### Verificar contenedor corriendo
```bash
docker ps
```

### Detener el contenedor
```bash
docker stop <CONTAINER_ID>
```

---

## ?? Git — Comandos frecuentes

```bash
# Ver historial de commits
git log --oneline

# Ver diferencias antes de commitear
git diff

# Deshacer cambios en un archivo (antes del commit)
git checkout -- webAPIAngular/Program.cs

# Ver ramas disponibles
git branch -a

# Crear y cambiar a nueva rama
git checkout -b feature/nueva-funcionalidad

# Fusionar rama a master
git checkout master
git merge feature/nueva-funcionalidad
```

---

## ?? Variables de entorno requeridas en Render

Configurar manualmente en **Render Dashboard ? relojes-lamur-api ? Environment**:

| Variable | Descripción |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión MySQL (Railway) |
| `Jwt__Secret` | Clave secreta para firmar tokens JWT |
| `Cors__AllowedOrigins__0` | URL del frontend Angular permitida |

Las siguientes se configuran automáticamente desde `render.yaml`:

| Variable | Valor |
|----------|-------|
| `PORT` | `8080` |
| `ASPNETCORE_URLS` | `http://+:8080` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Jwt__Issuer` | `RelojesLamurAPI` |
| `Jwt__Audience` | `RelojesLamurClient` |
| `Jwt__ExpirationDays` | `7` |

---

## ?? URLs importantes

| Recurso | URL |
|---------|-----|
| API en producción | https://relojeslamurapi.onrender.com |
| Swagger UI | https://relojeslamurapi.onrender.com |
| Health Check | https://relojeslamurapi.onrender.com/health |
| Render Dashboard | https://dashboard.render.com |
| Repositorio GitHub | https://github.com/cl-lamur/relojesLamurApi |
