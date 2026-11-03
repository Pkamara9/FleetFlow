# FleetFlow

Fleet maintenance SaaS: ASP.NET Core 8 API (`FleetFlow.Api`) + React/Vite/TypeScript web app (`web`).
See [docs/DATABASE.md](docs/DATABASE.md) for the data model.

## Features
Login (JWT + refresh token, auto-refresh on 401), dashboard, vehicles (list/search/detail/create/edit/delete),
maintenance schedules (due soon / overdue), service records, work orders (create, assign, change status), reports.
Roles: **Admin/Manager** can create/edit; **Technician** can update work order status and log services.

## Run locally
Prerequisites: .NET 8 SDK, Node 20+.

```bash
# 1. API (http://localhost:5080, Swagger at /swagger)
# Without SQL Server, use the in-memory database (data resets on restart):
Database__UseInMemory=true ASPNETCORE_ENVIRONMENT=Development dotnet run --project FleetFlow.Api --urls http://localhost:5080
# With SQL Server/LocalDB, set ConnectionStrings:DefaultConnection in appsettings and omit Database__UseInMemory.
# (PowerShell: $env:Database__UseInMemory="true")

# 2. Web (http://localhost:5173, proxies /api to the API)
cd web
npm install
npm run dev
```

Seeded logins (created on first start): `admin@fleetflow.com / Admin123!`, `manager@fleetflow.com / Manager123!`,
`tech@fleetflow.com / Tech123!`.

Web config (`web/.env.example`): `VITE_PROXY_TARGET` (dev proxy target), `VITE_API_URL` (API base URL for production builds).

## Tests / build
```bash
dotnet test                       # API tests (in-memory DB)
cd web && npm test && npm run build
```

## Deploy
- **API** (e.g. Azure App Service): `dotnet publish FleetFlow.Api -c Release`. Set `ConnectionStrings__DefaultConnection`,
  `JwtSettings__Secret` (long random value), `JwtSettings__Issuer/Audience`, `Stripe__*`, and
  `Cors__Origins__0=https://your-web-origin`. Do not ship the development secrets in `appsettings.json`.
- **Web** (Azure Static Web Apps, Netlify, S3/CloudFront, nginx…): build with
  `VITE_API_URL=https://your-api-host npm run build` and serve `web/dist`; configure an SPA fallback to `index.html`.

## API additions for the UI
`GET /api/auth/me` (full user), `GET /api/users`, `GET/POST /api/servicetypes`, `GET/POST /api/servicerecords`
(logging a service updates vehicle mileage and rolls the schedule forward), `PATCH /api/workorders/{id}/status`,
`?vehicleId=` filter on work orders/service records, real monthly cost chart on the dashboard, and 400/401 error codes.
