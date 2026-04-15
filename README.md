# Boardom

Real-time household sensor monitoring dashboard built with ASP.NET Core 8.0 Blazor Server.

**Live website:** [Website Link](https://boardom-dashboard.mercantec.tech/)

## Features

- **Live Device Monitoring** — View temperature, humidity, light, and pressure readings from connected sensors
- **Device & Group Management** — Organize devices into groups, add/edit/remove devices
- **Analytics** — Chart historical sensor data with date filtering, data thinning, and min/max/avg calculations
- **Authentication** — Auth0 OpenID Connect integration

## Prerequisites

- [Git](https://git-scm.com/)
- [Docker](https://www.docker.com/) and Docker Compose
- An [Auth0](https://auth0.com/) account with a configured application

## Getting Started

The Boardom project consists of two repositories that each run their own Docker Compose stack:

| Repository | Description | URL |
| --- | --- | --- |
| **Backend** | API, database, and MQTT services | https://github.com/MathiasGermandsen/boardom-backend.git |
| **Frontend** | Blazor Server dashboard (this repo) | _you are here_ |

### 1. Clone both repositories

```bash
git clone https://github.com/MathiasGermandsen/boardom-backend.git
git clone <this-repo-url> boardom-frontend
```

### 2. Start the backend

```bash
cd boardom-backend
```

Create a `.env` file in the backend root with the required environment variables (see the backend README for details), then run:

```bash
docker compose up -d
```

Wait until all backend services are healthy before continuing.

### 3. Start the frontend

```bash
cd ../boardom-frontend
```

Copy the example environment file and fill in your values:

```bash
cp .env.example .env
```

Then edit `.env` with your actual credentials (see [`.env.example`](.env.example) for the template).

Then run:

```bash
docker compose up -d
```

### 4. Open the app

Once both stacks are running, open the frontend in your browser:

```
http://localhost:13000
```

## Environment Variables (Frontend)

| Variable | Description |
| --- | --- |
| `AUTH0_DOMAIN` | Auth0 tenant domain |
| `AUTH0_CLIENT_ID` | Auth0 application client ID |
| `AUTH0_CLIENT_SECRET` | Auth0 application client secret |
| `AUTH0_AUDIENCE` | Auth0 API audience |
| `IP_ADDRESS_PORT` | Backend database API base URL (e.g. `http://host:8080/`) |
| `POWER_API_URL` | Backend power API base URL (e.g. `http://host:9090`) |

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
