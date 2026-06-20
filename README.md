# MovieNight

A microservices-based movie social platform. Users can discover films and TV series, rate and review them, build watchlists, manage friends, and send messages — all through a single Next.js frontend backed by a .NET 10 API Gateway and a suite of independent services.

---

## Architecture

```
Browser (Next.js :3000)
       │
       ▼
 MovieNight.Gateway (:7000)   ← single entry point, JWT validation, routing
       │
       ├── Auth.API      (:7010)   token issuance / refresh
       ├── Users.API     (:7001)   user profiles
       ├── Media.API     (:7002)   movies, series, search
       ├── Access.API    (:7003)   roles & permissions
       ├── Friends.API   (:7004)   friend relationships
       ├── MoviePlayer.API (:7005) playback sessions
       ├── People.API    (:7006)   cast & crew
       ├── Bookmark.API  (:7007)   watchlists / bookmarks
       ├── MovieRatings.API (:7008) star ratings
       ├── Review.API    (:7011)   written reviews
       ├── Messages.API  (:7020)   private messaging
       └── Achievements.API (:5102) badges & achievements
```

**Stack:**
- **Backend:** .NET 10, ASP.NET Core, Entity Framework Core (code-first migrations run on startup)
- **Database:** SQL Server 2022 (Docker), one database per service
- **Frontend:** Next.js 15, TypeScript, Tailwind CSS, shadcn/ui
- **Infrastructure:** Docker (SQL Server), Kubernetes manifests included for production

---

## Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| Docker Desktop | Latest | https://www.docker.com/products/docker-desktop |
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Node.js | 18+ | https://nodejs.org |
| Git Bash | Any | Included with Git for Windows |

> **Windows users:** the shell scripts (`start-local.sh`, `stop-local.sh`) are written for Git Bash. Run them from a Git Bash terminal, not PowerShell or cmd.

---

## Quick Start

```bash
# Clone
git clone <repo-url>
cd MovieNight

# Start everything (Git Bash)
./start-local.sh
```

The script handles the entire local setup:

1. Checks that dotnet, docker, node, and npm are available.
2. Generates `.secrets.local` with random `SA_PASSWORD`, `JWT_SECRET`, and `GATEWAY_INTERNAL_SECRET` if it does not already exist.
3. Writes `appsettings.Development.json` for every service and the Gateway using those secrets.
4. Writes `MovieNight.UI/clientapp/.env.local` for the frontend.
5. Starts (or restarts) the `movienight_mssql` Docker container and waits for SQL Server to be ready.
6. Stops any previously running service processes.
7. Runs `dotnet restore` + `dotnet build`.
8. Runs `npm install` if `node_modules` is missing.
9. Launches all .NET services in hidden background windows (logs go to `.runtime/*.log`).
10. Launches the Next.js dev server in the background.
11. Prints all service URLs.

Once it finishes, open **http://localhost:3000**.

---

## Service URLs

| Service | URL | Swagger/Scalar |
|---------|-----|---------------|
| Frontend | http://localhost:3000 | — |
| Gateway | http://localhost:7000 | http://localhost:7000/scalar/v1 |
| Auth.API | http://localhost:7010 | http://localhost:7010/scalar/v1 |
| Users.API | http://localhost:7001 | http://localhost:7001/scalar/v1 |
| Media.API | http://localhost:7002 | http://localhost:7002/scalar/v1 |
| Access.API | http://localhost:7003 | http://localhost:7003/scalar/v1 |
| Friends.API | http://localhost:7004 | http://localhost:7004/scalar/v1 |
| MoviePlayer.API | http://localhost:7005 | http://localhost:7005/scalar/v1 |
| People.API | http://localhost:7006 | http://localhost:7006/scalar/v1 |
| Bookmark.API | http://localhost:7007 | http://localhost:7007/scalar/v1 |
| MovieRatings.API | http://localhost:7008 | http://localhost:7008/scalar/v1 |
| Review.API | http://localhost:7011 | http://localhost:7011/scalar/v1 |
| Messages.API | http://localhost:7020 | http://localhost:7020/scalar/v1 |
| Achievements.API | http://localhost:5102 | http://localhost:5102/scalar/v1 |

---

## Stop Services

```bash
# Stop all .NET services and the frontend (leave SQL Server running)
./stop-local.sh

# Also stop and remove the SQL Server Docker container
./stop-local.sh --docker
```

---

## Manual Setup

If you prefer to start services individually without the script, follow these steps.

### 1. Create `.secrets.local`

Create the file at the project root with the following format:

```
SA_PASSWORD=YourStrong!Passw0rd
JWT_SECRET=your-base64url-encoded-secret-at-least-32-chars
GATEWAY_INTERNAL_SECRET=another-random-secret
```

> `.secrets.local` is listed in `.gitignore` and will never be committed.

### 2. Start SQL Server (Docker)

```bash
docker run -d \
  --name movienight_mssql \
  -e ACCEPT_EULA=Y \
  -e SA_PASSWORD=YourStrong!Passw0rd \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

Wait ~15 seconds for the container to be fully ready.

### 3. Write `appsettings.Development.json` per service

Each service with a database needs its own override file. Place it next to the service's `appsettings.json`.

**Gateway** (`MovieNight.Gateway/appsettings.Development.json`):
```json
{
  "AUTH_JWT_ISSUER": "MovieNight.Auth",
  "AUTH_JWT_AUDIENCE": "MovieNight.Client",
  "AUTH_JWT_SECRET": "<JWT_SECRET>",
  "Gateway": { "InternalSecret": "<GATEWAY_INTERNAL_SECRET>" }
}
```

**Auth.API** (`Services/Auth/Auth.API/appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "TokensDb": "Server=localhost,1433;Database=MN_Tokens;User Id=sa;Password=<SA_PASSWORD>;TrustServerCertificate=True"
  },
  "AUTH_JWT_ISSUER": "MovieNight.Auth",
  "AUTH_JWT_AUDIENCE": "MovieNight.Client",
  "AUTH_JWT_SECRET": "<JWT_SECRET>",
  "Gateway": { "BaseUrl": "http://localhost:7000" }
}
```

**All other services with a DB** (replace `<DbKey>`, `<DbName>`, and `<SA_PASSWORD>`):
```json
{
  "ConnectionStrings": {
    "<DbKey>": "Server=localhost,1433;Database=MN_<DbName>;User Id=sa;Password=<SA_PASSWORD>;TrustServerCertificate=True"
  }
}
```

| Service | DbKey | DbName |
|---------|-------|--------|
| Access.API | AccessDb | Access |
| Bookmark.API | BookmarksDb | Bookmarks |
| Friends.API | FriendsDb | Friends |
| Media.API | MediaDb | Media |
| Messages.API | MessagesDb | Messages |
| MoviePlayer.API | MoviePlayerDb | MoviePlayer |
| MovieRatings.API | RatingsDb | Ratings |
| People.API | PeopleDb | People |
| Review.API | ReviewDb | Review |
| Users.API | UsersDb | Users |

> Achievements.API has no database — no override file is needed.

### 4. Build the solution

```bash
dotnet restore MovieNight.sln
dotnet build MovieNight.sln -c Debug
```

### 5. Install frontend dependencies

```bash
cd MovieNight.UI/clientapp
npm install
```

### 6. Write `MovieNight.UI/clientapp/.env.local`

```
NEXT_PUBLIC_GATEWAY_URL=http://localhost:7000
```

### 7. Start each service

Open a separate terminal for each service (or use Windows Terminal tabs):

```bash
# Auth.API — start first
cd Services/Auth/Auth.API
dotnet run --launch-profile http

# Then start the rest in any order:
cd Services/User/Users.API             && dotnet run --launch-profile http
cd Services/Media/Media.API            && dotnet run --launch-profile http
cd Services/Access/Access.API          && dotnet run --launch-profile http
cd Services/Friends/Friends.API        && dotnet run --launch-profile http
cd Services/MoviePlayer/MoviePlayer.API && dotnet run --launch-profile http
cd Services/People/People.API          && dotnet run --launch-profile http
cd Services/Bookmark/Bookmark.API      && dotnet run --launch-profile http
cd Services/MovieRatings/MovieRatings.API && dotnet run --launch-profile http
cd Services/Review/Review.API          && dotnet run --launch-profile http
cd Services/Messages/Messages.API      && dotnet run --launch-profile http
cd Services/Achievements/Achievements.API && dotnet run --launch-profile http

# Gateway — start after backend services are up
cd MovieNight.Gateway                  && dotnet run --launch-profile http

# Frontend
cd MovieNight.UI/clientapp             && npm run dev
```

Each service runs EF Core migrations automatically on startup (`MigrateWithRetryAsync` pattern) — no manual `dotnet ef database update` is required.

---

## Project Structure

```
MovieNight/
├── MovieNight.Gateway/          # YARP reverse proxy + JWT middleware
├── MovieNight.UI/
│   └── clientapp/               # Next.js 15 frontend
├── Services/
│   ├── Access/Access.API/
│   ├── Achievements/Achievements.API/
│   ├── Auth/Auth.API/
│   ├── Bookmark/Bookmark.API/
│   ├── Friends/Friends.API/
│   ├── Media/Media.API/
│   ├── Messages/Messages.API/
│   ├── MoviePlayer/MoviePlayer.API/
│   ├── MovieRatings/MovieRatings.API/
│   ├── People/People.API/
│   ├── Review/Review.API/
│   └── User/Users.API/
├── .runtime/                    # Generated at runtime: .pid, .bat, .log files
├── .secrets.local               # Local secrets — gitignored, never commit
├── start-local.sh               # One-command local startup (Git Bash)
├── stop-local.sh                # Graceful shutdown
└── MovieNight.sln
```

---

## Environment Variables Reference

| Variable | Used by | Description |
|----------|---------|-------------|
| `AUTH_JWT_ISSUER` | Gateway, Auth.API | JWT issuer claim (`MovieNight.Auth`) |
| `AUTH_JWT_AUDIENCE` | Gateway, Auth.API | JWT audience claim (`MovieNight.Client`) |
| `AUTH_JWT_SECRET` | Gateway, Auth.API | Signing secret for JWT tokens |
| `Gateway:InternalSecret` | Gateway | Shared secret for gateway-to-service calls |
| `Gateway:BaseUrl` | Auth.API | Gateway base URL for callback registration |
| `SA_PASSWORD` | Docker, all services | SQL Server `sa` account password |

---

## Docker Compose / Kubernetes (Production)

Kubernetes manifests for production deployment are located in the repository under the infrastructure directory. For local containerised runs, refer to any `docker-compose.yml` at the project root or the Kubernetes YAML files for full service definitions.

---

## Notes

- **`.secrets.local` is gitignored.** Never commit it. If you rotate secrets, delete the file and re-run `./start-local.sh` to regenerate everything.
- **Log files** for background services are written to `.runtime/<ServiceName>.log`. Tail them with:
  ```bash
  tail -f .runtime/Auth.API.log
  ```
- **Database migrations** run automatically on each service startup. If a migration fails (e.g., SQL Server not yet ready), services retry with exponential backoff (`MigrateWithRetryAsync`).
- **Port conflicts:** if any port is already in use, identify the owner and stop the conflicting process before running `./start-local.sh`:
  ```bash
  # Git Bash / PowerShell
  netstat -ano | findstr :<port>
  ```
