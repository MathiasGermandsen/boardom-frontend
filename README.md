# Boardom

Real-time household sensor monitoring dashboard built with ASP.NET Core 8.0 Blazor Server.

## Features

- **Live Device Monitoring** — View temperature, humidity, light, and pressure readings from connected sensors
- **Device & Group Management** — Organize devices into groups, add/edit/remove devices
- **Analytics** — Chart historical sensor data with date filtering, data thinning, and min/max/avg calculations
- **Power Settings** — Configure energy company and price-per-kWh preferences
- **Authentication** — Auth0 OpenID Connect integration

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (for containerized deployment)
- An [Auth0](https://auth0.com/) account with a configured application

## Getting Started

### Local Development

1. Clone the repository
2. Configure Auth0 credentials in `Boardom/appsettings.json` or via environment variables:
   ```
   Auth0__Domain=<your-auth0-domain>
   Auth0__ClientId=<your-client-id>
   Auth0__ClientSecret=<your-client-secret>
   ```
3. Configure API URLs:
   ```
   ApiSettings__DatabaseApiUrl=<database-api-url>
   ApiSettings__PowerApiUrl=<power-api-url>
   ```
4. Run the application:
   ```bash
   cd Boardom
   dotnet run
   ```
5. Open https://localhost:7086 (or http://localhost:5131)

### Docker

1. Create a `.env` file in the project root (see [Environment Variables](#environment-variables))
2. Build and run:
   ```bash
   docker compose up -d
   ```
3. The app will be available at http://localhost:13000

## Environment Variables

| Variable              | Description                                      |
| --------------------- | ------------------------------------------------ |
| `AUTH0_DOMAIN`        | Auth0 tenant domain                              |
| `AUTH0_CLIENT_ID`     | Auth0 application client ID                      |
| `AUTH0_CLIENT_SECRET` | Auth0 application client secret                  |
| `IP_ADDRESS_PORT`     | Database API base URL (e.g. `http://host:8080/`) |
| `POWER_API_URL`       | Power API base URL (e.g. `http://host:9090`)     |

## Project Structure

```
Boardom/
├── Components/
│   ├── Layout/          # MainLayout, NavMenu
│   └── Pages/           # Home, Devices, Analytics, Power, Login, Error
├── Controllers/         # AccountController (auth), DeviceController (API)
├── Models/              # Request/response DTOs
├── Services/            # DeviceFunctions, DeviceStatus, PowerService, PendingDeviceStore
├── wwwroot/             # Static assets (CSS, JS)
└── Program.cs           # App configuration and middleware
```

## Tech Stack

- ASP.NET Core 8.0 — Blazor Server (interactive server rendering)
- Auth0 — Authentication
- Chart.js — Analytics charting
- Bootstrap — UI framework
