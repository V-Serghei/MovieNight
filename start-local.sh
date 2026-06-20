#!/usr/bin/env bash
set -euo pipefail

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
BLUE='\033[0;34m'; CYAN='\033[0;36m'; BOLD='\033[1m'; NC='\033[0m'

ok()      { echo -e "${GREEN}✓${NC} $1"; }
info()    { echo -e "${YELLOW}→${NC} $1"; }
err()     { echo -e "${RED}✗ ERROR:${NC} $1"; exit 1; }
section() { echo -e "\n${BOLD}${BLUE}── $1 ──${NC}"; }

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RUNTIME_DIR="$ROOT/.runtime"
SECRETS_FILE="$ROOT/.secrets.local"
CONTAINER="movienight_mssql"

mkdir -p "$RUNTIME_DIR"

# ─────────────────────────────────────────────
section "Prerequisites"
# ─────────────────────────────────────────────

check_dotnet() {
  if ! command -v dotnet &>/dev/null; then
    err "dotnet is not installed. Install .NET 10 SDK from https://dotnet.microsoft.com/download"
  fi
  local ver
  ver=$(dotnet --version 2>/dev/null | cut -d. -f1)
  if [ "$ver" -lt 10 ]; then
    err "dotnet 10+ required (found $(dotnet --version)). Install .NET 10 SDK."
  fi
  ok "dotnet $(dotnet --version)"
}

check_docker() {
  if ! command -v docker &>/dev/null; then
    err "docker is not installed. Install Docker Desktop from https://www.docker.com/products/docker-desktop"
  fi
  if ! docker info &>/dev/null; then
    err "Docker daemon is not running. Start Docker Desktop."
  fi
  ok "docker $(docker --version | awk '{print $3}' | tr -d ',')"
}

check_node() {
  if ! command -v node &>/dev/null; then
    err "node is not installed. Install Node.js 18+ from https://nodejs.org"
  fi
  local ver
  ver=$(node --version | tr -d 'v' | cut -d. -f1)
  if [ "$ver" -lt 18 ]; then
    err "Node.js 18+ required (found $(node --version)). Install from https://nodejs.org"
  fi
  ok "node $(node --version)"
}

check_npm() {
  if ! command -v npm &>/dev/null; then
    err "npm is not installed. It should come bundled with Node.js."
  fi
  ok "npm $(npm --version)"
}

check_dotnet
check_docker
check_node
check_npm

# ─────────────────────────────────────────────
section "Secrets"
# ─────────────────────────────────────────────

generate_secret() {
  node -e "process.stdout.write(require('crypto').randomBytes(32).toString('base64url'))"
}

if [ ! -f "$SECRETS_FILE" ]; then
  info "No .secrets.local found — generating one..."
  SA_PASSWORD="MovieNight_$(generate_secret | head -c 16)1!"
  JWT_SECRET="$(generate_secret)"
  GATEWAY_INTERNAL_SECRET="$(generate_secret)"
  cat > "$SECRETS_FILE" <<EOF
SA_PASSWORD=${SA_PASSWORD}
JWT_SECRET=${JWT_SECRET}
GATEWAY_INTERNAL_SECRET=${GATEWAY_INTERNAL_SECRET}
EOF
  ok "Created .secrets.local with generated secrets"
else
  ok "Loaded .secrets.local"
fi

# Source the secrets file
while IFS='=' read -r key value; do
  [[ -z "$key" || "$key" =~ ^# ]] && continue
  export "$key"="$value"
done < "$SECRETS_FILE"

: "${SA_PASSWORD:?SA_PASSWORD not set in .secrets.local}"
: "${JWT_SECRET:?JWT_SECRET not set in .secrets.local}"
: "${GATEWAY_INTERNAL_SECRET:?GATEWAY_INTERNAL_SECRET not set in .secrets.local}"

# ─────────────────────────────────────────────
section "Writing appsettings.Development.json"
# ─────────────────────────────────────────────

write_appsettings() {
  local path="$1"
  local content="$2"
  mkdir -p "$(dirname "$path")"
  echo "$content" > "$path"
  ok "Wrote $path"
}

# Gateway
write_appsettings "$ROOT/MovieNight.Gateway/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "AUTH_JWT_ISSUER": "MovieNight.Auth",
  "AUTH_JWT_AUDIENCE": "MovieNight.Client",
  "AUTH_JWT_SECRET": "'"$JWT_SECRET"'",
  "Gateway": { "InternalSecret": "'"$GATEWAY_INTERNAL_SECRET"'" }
}'

# Auth.API (TokensDb)
write_appsettings "$ROOT/Services/Auth/Auth.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "TokensDb": "Server=localhost,1433;Database=MN_Tokens;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  },
  "AUTH_JWT_ISSUER": "MovieNight.Auth",
  "AUTH_JWT_AUDIENCE": "MovieNight.Client",
  "AUTH_JWT_SECRET": "'"$JWT_SECRET"'",
  "Gateway": { "BaseUrl": "http://localhost:7000" }
}'

# Access.API (AccessDb)
write_appsettings "$ROOT/Services/Access/Access.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "AccessDb": "Server=localhost,1433;Database=MN_Access;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Bookmark.API (BookmarksDb)
write_appsettings "$ROOT/Services/Bookmark/Bookmark.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "BookmarksDb": "Server=localhost,1433;Database=MN_Bookmarks;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Friends.API (FriendsDb)
write_appsettings "$ROOT/Services/Friends/Friends.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "FriendsDb": "Server=localhost,1433;Database=MN_Friends;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Media.API (MediaDb)
write_appsettings "$ROOT/Services/Media/Media.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MediaDb": "Server=localhost,1433;Database=MN_Media;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Messages.API (MessagesDb)
write_appsettings "$ROOT/Services/Messages/Messages.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MessagesDb": "Server=localhost,1433;Database=MN_Messages;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# MoviePlayer.API (MoviePlayerDb)
write_appsettings "$ROOT/Services/MoviePlayer/MoviePlayer.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MoviePlayerDb": "Server=localhost,1433;Database=MN_MoviePlayer;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# MovieRatings.API (RatingsDb)
write_appsettings "$ROOT/Services/MovieRatings/MovieRatings.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "RatingsDb": "Server=localhost,1433;Database=MN_Ratings;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# People.API (PeopleDb)
write_appsettings "$ROOT/Services/People/People.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PeopleDb": "Server=localhost,1433;Database=MN_People;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Review.API (ReviewDb)
write_appsettings "$ROOT/Services/Review/Review.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ReviewDb": "Server=localhost,1433;Database=MN_Review;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Users.API (UsersDb)
write_appsettings "$ROOT/Services/User/Users.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "UsersDb": "Server=localhost,1433;Database=MN_Users;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Frontend .env.local
ENVLOCAL="$ROOT/MovieNight.UI/clientapp/.env.local"
cat > "$ENVLOCAL" <<EOF
NEXT_PUBLIC_GATEWAY_URL=http://localhost:7000
EOF
ok "Wrote $ENVLOCAL"

# ─────────────────────────────────────────────
section "SQL Server (Docker)"
# ─────────────────────────────────────────────

if docker inspect "$CONTAINER" &>/dev/null; then
  local_state
  local_state=$(docker inspect -f '{{.State.Status}}' "$CONTAINER")
  if [ "$local_state" = "running" ]; then
    info "Container '$CONTAINER' already running — restarting to apply fresh password..."
    docker rm -f "$CONTAINER" &>/dev/null
  else
    info "Removing stopped container '$CONTAINER'..."
    docker rm -f "$CONTAINER" &>/dev/null
  fi
fi

info "Starting SQL Server container..."
docker run -d \
  --name "$CONTAINER" \
  -e ACCEPT_EULA=Y \
  -e SA_PASSWORD="$SA_PASSWORD" \
  -e MSSQL_SA_PASSWORD="$SA_PASSWORD" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest \
  &>/dev/null
ok "Container '$CONTAINER' started"

info "Waiting for SQL Server to be ready..."
MAX_WAIT=60
WAITED=0
until docker exec "$CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -No &>/dev/null 2>&1; do
  if [ $WAITED -ge $MAX_WAIT ]; then
    err "SQL Server did not become ready after ${MAX_WAIT}s. Check: docker logs $CONTAINER"
  fi
  sleep 2
  WAITED=$((WAITED + 2))
done
ok "SQL Server is ready (${WAITED}s)"

# ─────────────────────────────────────────────
section "Stop existing services"
# ─────────────────────────────────────────────

stop_existing() {
  info "Killing any previously running .NET services..."

  # Kill by PID files
  if ls "$RUNTIME_DIR"/*.pid &>/dev/null 2>&1; then
    for pidfile in "$RUNTIME_DIR"/*.pid; do
      pid=$(cat "$pidfile" 2>/dev/null || true)
      if [ -n "$pid" ]; then
        powershell.exe -NoProfile -Command "
          try { Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue } catch {}
        " &>/dev/null 2>&1 || true
      fi
      rm -f "$pidfile"
    done
    ok "Stopped processes from .pid files"
  fi

  # Kill by process name patterns (belt-and-suspenders)
  local patterns=(
    "Access.API" "Auth.API" "Bookmark.API" "Friends.API"
    "Media.API" "Messages.API" "MoviePlayer.API" "MovieRatings.API"
    "People.API" "Review.API" "Users.API" "Achievements.API"
    "MovieNight.Gateway"
  )
  for pattern in "${patterns[@]}"; do
    powershell.exe -NoProfile -Command "
      Get-Process | Where-Object { \$_.MainWindowTitle -like '*${pattern}*' -or \$_.ProcessName -like '*${pattern}*' } |
        Stop-Process -Force -ErrorAction SilentlyContinue
    " &>/dev/null 2>&1 || true
  done

  # Also kill by dotnet process command lines containing service paths
  powershell.exe -NoProfile -Command "
    Get-WmiObject Win32_Process -Filter \"Name='dotnet.exe'\" | ForEach-Object {
      \$cmd = \$_.CommandLine
      if (\$cmd -match 'Access\.API|Auth\.API|Bookmark\.API|Friends\.API|Media\.API|Messages\.API|MoviePlayer\.API|MovieRatings\.API|People\.API|Review\.API|Users\.API|Achievements\.API|MovieNight\.Gateway') {
        Stop-Process -Id \$_.ProcessId -Force -ErrorAction SilentlyContinue
      }
    }
  " &>/dev/null 2>&1 || true

  sleep 1
  ok "Service cleanup done"
}

stop_existing

# ─────────────────────────────────────────────
section "Build"
# ─────────────────────────────────────────────

info "Restoring NuGet packages..."
dotnet restore "$ROOT/MovieNight.sln" -q
ok "Packages restored"

info "Building solution..."
dotnet build "$ROOT/MovieNight.sln" --no-restore -c Debug -q
ok "Build succeeded"

# ─────────────────────────────────────────────
section "Frontend dependencies"
# ─────────────────────────────────────────────

CLIENTAPP="$ROOT/MovieNight.UI/clientapp"
if [ ! -d "$CLIENTAPP/node_modules" ]; then
  info "Installing npm packages..."
  (cd "$CLIENTAPP" && npm install --silent)
  ok "npm packages installed"
else
  ok "node_modules already present — skipping npm install"
fi

# ─────────────────────────────────────────────
section "Starting services"
# ─────────────────────────────────────────────

# Helper: write a .bat wrapper and launch it hidden via PowerShell, save PID
start_service() {
  local name="$1"
  local project_path="$2"
  local port="$3"

  local bat_file="$RUNTIME_DIR/${name}.bat"
  local pid_file="$RUNTIME_DIR/${name}.pid"
  local log_file="$RUNTIME_DIR/${name}.log"

  # Write wrapper bat
  cat > "$bat_file" <<BATEOF
@echo off
cd /d "${project_path//\//\\}"
dotnet run --launch-profile http --no-build >> "${log_file//\//\\}" 2>&1
BATEOF

  # Launch hidden window via PowerShell, capture PID of the cmd.exe process
  local pid
  pid=$(powershell.exe -NoProfile -Command "
    \$p = Start-Process -FilePath 'cmd.exe' \`
      -ArgumentList '/c', '\"${bat_file//\//\\}\"' \`
      -WindowStyle Hidden \`
      -PassThru
    \$p.Id
  " 2>/dev/null | tr -d '\r')

  echo "$pid" > "$pid_file"
  ok "Started $name on :$port (PID $pid)"
}

wait_for_http() {
  local name="$1"
  local url="$2"
  local max_wait="${3:-60}"
  local waited=0
  info "Waiting for $name to respond at $url..."
  until curl -sf "$url/health" &>/dev/null || curl -sf "$url" &>/dev/null; do
    if [ $waited -ge $max_wait ]; then
      echo -e "${YELLOW}⚠ $name did not respond after ${max_wait}s — continuing anyway${NC}"
      return 0
    fi
    sleep 2
    waited=$((waited + 2))
  done
  ok "$name is responding (${waited}s)"
}

# Auth.API must start first — other services depend on JWT config being consistent
start_service "Auth.API"        "$ROOT/Services/Auth/Auth.API"              7010
wait_for_http  "Auth.API"       "http://localhost:7010"                       60

# Core data services
start_service "Users.API"       "$ROOT/Services/User/Users.API"             7001
start_service "Media.API"       "$ROOT/Services/Media/Media.API"            7002
start_service "Access.API"      "$ROOT/Services/Access/Access.API"          7003

# Social services
start_service "Friends.API"     "$ROOT/Services/Friends/Friends.API"        7004
start_service "MoviePlayer.API" "$ROOT/Services/MoviePlayer/MoviePlayer.API" 7005
start_service "People.API"      "$ROOT/Services/People/People.API"          7006
start_service "Bookmark.API"    "$ROOT/Services/Bookmark/Bookmark.API"      7007
start_service "MovieRatings.API" "$ROOT/Services/MovieRatings/MovieRatings.API" 7008
start_service "Review.API"      "$ROOT/Services/Review/Review.API"          7011
start_service "Messages.API"    "$ROOT/Services/Messages/Messages.API"      7020

# Achievements (no DB)
start_service "Achievements.API" "$ROOT/Services/Achievements/Achievements.API" 5102

# Wait briefly for the core services to start before Gateway
sleep 5

# Gateway (depends on all backend services being routable)
start_service "Gateway"         "$ROOT/MovieNight.Gateway"                  7000

# ─────────────────────────────────────────────
section "Starting frontend"
# ─────────────────────────────────────────────

FRONTEND_BAT="$RUNTIME_DIR/Frontend.bat"
FRONTEND_PID="$RUNTIME_DIR/Frontend.pid"
FRONTEND_LOG="$RUNTIME_DIR/Frontend.log"

cat > "$FRONTEND_BAT" <<BATEOF
@echo off
cd /d "${CLIENTAPP//\//\\}"
npm run dev >> "${FRONTEND_LOG//\//\\}" 2>&1
BATEOF

FRONTEND_PID_VAL=$(powershell.exe -NoProfile -Command "
  \$p = Start-Process -FilePath 'cmd.exe' \`
    -ArgumentList '/c', '\"${FRONTEND_BAT//\//\\}\"' \`
    -WindowStyle Hidden \`
    -PassThru
  \$p.Id
" 2>/dev/null | tr -d '\r')

echo "$FRONTEND_PID_VAL" > "$FRONTEND_PID"
ok "Started Frontend on :3000 (PID $FRONTEND_PID_VAL)"

# ─────────────────────────────────────────────
section "All services started"
# ─────────────────────────────────────────────

echo ""
echo -e "${BOLD}${CYAN}Service URLs:${NC}"
echo -e "  ${CYAN}Frontend         ${NC}→  http://localhost:3000"
echo -e "  ${CYAN}Gateway          ${NC}→  http://localhost:7000"
echo -e "  ${CYAN}Auth.API         ${NC}→  http://localhost:7010"
echo -e "  ${CYAN}Users.API        ${NC}→  http://localhost:7001"
echo -e "  ${CYAN}Media.API        ${NC}→  http://localhost:7002"
echo -e "  ${CYAN}Access.API       ${NC}→  http://localhost:7003"
echo -e "  ${CYAN}Friends.API      ${NC}→  http://localhost:7004"
echo -e "  ${CYAN}MoviePlayer.API  ${NC}→  http://localhost:7005"
echo -e "  ${CYAN}People.API       ${NC}→  http://localhost:7006"
echo -e "  ${CYAN}Bookmark.API     ${NC}→  http://localhost:7007"
echo -e "  ${CYAN}MovieRatings.API ${NC}→  http://localhost:7008"
echo -e "  ${CYAN}Review.API       ${NC}→  http://localhost:7011"
echo -e "  ${CYAN}Messages.API     ${NC}→  http://localhost:7020"
echo -e "  ${CYAN}Achievements.API ${NC}→  http://localhost:5102"
echo ""
echo -e "${BOLD}Logs:${NC} ${RUNTIME_DIR}/*.log"
echo -e "${BOLD}Stop:${NC} ./stop-local.sh"
echo ""
ok "MovieNight is running. Open http://localhost:3000"
