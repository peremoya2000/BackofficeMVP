# Altairis · Backoffice de operaciones

MVP del backoffice de operaciones de Viajes Altairis: hoteles, tipos de habitación, inventario (cupos), reservas y un panel de operaciones.

## Puesta en marcha

Solo hace falta Docker (Docker Desktop o Docker Engine con Compose v2):

```bash
docker compose up --build
```

Cuando los tres servicios aparezcan como `healthy`, abre **http://localhost:8080**.

| Servicio | Qué es | Acceso |
|---|---|---|
| `web` | Angular servido por nginx, que reenvía `/api` a la API | http://localhost:8080 |
| `api` | API REST en .NET 10 | a través de `web`: http://localhost:8080/api/health |
| `db` | PostgreSQL 18 | `localhost:5433` (solo desde esta máquina) |

Las credenciales de la base de datos están en `docker-compose.yml` para que todo arranque sin configuración adicional. **Son solo para la demo.**

Para volver a empezar con la base de datos vacía: `docker compose down -v`.

## Desarrollo local

Requisitos: .NET SDK 10 y Node.js 24 LTS o superior.

1. Base de datos: `docker compose up db`
2. API (desde `backend/`): `dotnet run --project src/Altairis.Backoffice.Api` → http://localhost:5012
3. Frontend (desde `frontend/`): `npm install` y `npm start` → http://localhost:4200. El proxy de desarrollo (`proxy.conf.json`) reenvía `/api` a la API, igual que nginx en Docker.

Tests (desde `backend/`): `dotnet test`
