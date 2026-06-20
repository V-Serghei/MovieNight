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
SQL_HOST_PORT="${SQL_HOST_PORT:-11433}"

winpath() {
  cygpath -w "$1"
}

ROOT_WIN="$(winpath "$ROOT")"
CLIENTAPP_WIN="$(winpath "$ROOT/MovieNight.UI/clientapp")"

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
  "Services": {
    "Auth": "http://localhost:7010",
    "Users": "http://localhost:7001",
    "Media": "http://localhost:7002",
    "Access": "http://localhost:7003",
    "Friends": "http://localhost:7004",
    "MoviePlayer": "http://localhost:7005",
    "People": "http://localhost:7006",
    "Bookmarks": "http://localhost:7007",
    "Ratings": "http://localhost:7008",
    "Review": "http://localhost:7011",
    "Messages": "http://localhost:7020"
  },
  "Gateway": { "InternalSecret": "'"$GATEWAY_INTERNAL_SECRET"'" }
}'

# Auth.API (TokensDb)
write_appsettings "$ROOT/Services/Auth/Auth.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "TokensDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Tokens;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
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
    "AccessDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Access;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Bookmark.API (BookmarksDb)
write_appsettings "$ROOT/Services/Bookmark/Bookmark.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "BookmarksDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Bookmarks;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Friends.API (FriendsDb)
write_appsettings "$ROOT/Services/Friends/Friends.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "FriendsDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Friends;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Media.API (MediaDb)
write_appsettings "$ROOT/Services/Media/Media.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MediaDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Media;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Messages.API (MessagesDb)
write_appsettings "$ROOT/Services/Messages/Messages.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MessagesDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Messages;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# MoviePlayer.API (MoviePlayerDb)
write_appsettings "$ROOT/Services/MoviePlayer/MoviePlayer.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MoviePlayerDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_MoviePlayer;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# MovieRatings.API (RatingsDb)
write_appsettings "$ROOT/Services/MovieRatings/MovieRatings.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "RatingsDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Ratings;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# People.API (PeopleDb)
write_appsettings "$ROOT/Services/People/People.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PeopleDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_People;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Review.API (ReviewDb)
write_appsettings "$ROOT/Services/Review/Review.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ReviewDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Review;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Users.API (UsersDb)
write_appsettings "$ROOT/Services/User/Users.API/appsettings.Development.json" \
'{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "UsersDb": "Server=localhost,'"$SQL_HOST_PORT"';Database=MN_Users;User Id=sa;Password='"$SA_PASSWORD"';TrustServerCertificate=True"
  }
}'

# Frontend .env.local
# GATEWAY_URL is server-side only (used in /api/gw/[...path] proxy route)
ENVLOCAL="$ROOT/MovieNight.UI/clientapp/.env.local"
cat > "$ENVLOCAL" <<EOF
GATEWAY_URL=http://localhost:7000
EOF
ok "Wrote $ENVLOCAL"

# ─────────────────────────────────────────────
section "SQL Server (Docker)"
# ─────────────────────────────────────────────

CONTAINER_RUNNING=false

if docker inspect "$CONTAINER" &>/dev/null; then
  CONTAINER_STATE=$(docker inspect -f '{{.State.Status}}' "$CONTAINER")
  if [ "$CONTAINER_STATE" = "running" ]; then
    CURRENT_SQL_HOST_PORT=$(docker inspect -f '{{(index (index .NetworkSettings.Ports "1433/tcp") 0).HostPort}}' "$CONTAINER" 2>/dev/null || true)
    if [ "$CURRENT_SQL_HOST_PORT" = "$SQL_HOST_PORT" ]; then
      ok "Container '$CONTAINER' already running on :$SQL_HOST_PORT — reusing it"
      CONTAINER_RUNNING=true
    else
      info "Container '$CONTAINER' is mapped to :${CURRENT_SQL_HOST_PORT:-unknown}; recreating on :$SQL_HOST_PORT..."
      docker rm -f "$CONTAINER" &>/dev/null
    fi
  else
    info "Removing stopped container '$CONTAINER'..."
    docker rm -f "$CONTAINER" &>/dev/null
  fi
fi

if ! $CONTAINER_RUNNING; then
  info "Pulling SQL Server image (first run: ~1.5 GB, subsequent runs are instant)..."
  docker pull mcr.microsoft.com/mssql/server:2022-latest

  info "Starting SQL Server container..."
  docker run -d \
    --name "$CONTAINER" \
    -e ACCEPT_EULA=Y \
    -e SA_PASSWORD="$SA_PASSWORD" \
    -e MSSQL_SA_PASSWORD="$SA_PASSWORD" \
    -p "$SQL_HOST_PORT:1433" \
    mcr.microsoft.com/mssql/server:2022-latest \
    &>/dev/null
  ok "Container '$CONTAINER' started"
fi

info "Waiting for SQL Server to accept logins on localhost:${SQL_HOST_PORT}..."
MAX_WAIT=90
WAITED=0
until powershell.exe -NoProfile -Command "
  \$cs = 'Server=localhost,$SQL_HOST_PORT;Database=master;User Id=sa;Password=$SA_PASSWORD;TrustServerCertificate=True;Encrypt=True'
  Add-Type -AssemblyName System.Data
  \$c = New-Object System.Data.SqlClient.SqlConnection(\$cs)
  try { \$c.Open(); exit 0 } catch { exit 1 } finally { \$c.Dispose() }
" &>/dev/null 2>&1; do
  if [ $WAITED -ge $MAX_WAIT ]; then
    err "SQL Server did not accept logins on localhost:${SQL_HOST_PORT} after ${MAX_WAIT}s. Check: docker logs $CONTAINER"
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

  # PID files can be stale on Windows after a crash, so do not blindly kill
  # those IDs. The verified command-line scan below does the actual cleanup.
  if ls "$RUNTIME_DIR"/*.pid &>/dev/null 2>&1; then
    rm -f "$RUNTIME_DIR"/*.pid
    ok "Removed stale .pid files"
  fi

  # Also kill orphaned service processes from this workspace. This covers
  # direct apphost launches such as Auth.API.exe, not only dotnet.exe.
  powershell.exe -NoProfile -Command "
    \$root = [regex]::Escape('$ROOT_WIN')
    \$serviceNames = @(
      'Access.API.exe',
      'Auth.API.exe',
      'Achievements.API.exe',
      'Bookmark.API.exe',
      'Friends.API.exe',
      'Media.API.exe',
      'Messages.API.exe',
      'MoviePlayer.API.exe',
      'MovieRatings.API.exe',
      'People.API.exe',
      'Review.API.exe',
      'Users.API.exe',
      'MovieNight.Gateway.exe'
    )
    \$servicePattern = 'Access\.API|Auth\.API|Bookmark\.API|Friends\.API|Media\.API|Messages\.API|MoviePlayer\.API|MovieRatings\.API|People\.API|Review\.API|Users\.API|Achievements\.API|MovieNight\.Gateway'
    Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | ForEach-Object {
      if (\$_.CommandLine -match \$root -and (
          \$serviceNames -contains \$_.Name -or
          (\$_.Name -eq 'dotnet.exe' -and \$_.CommandLine -match \$servicePattern)
        )) {
          Stop-Process -Id \$_.ProcessId -Force -ErrorAction SilentlyContinue
      }
    }
  " &>/dev/null 2>&1 || true

  powershell.exe -NoProfile -Command "
    \$root = [regex]::Escape('$ROOT_WIN')
    \$rootUnix = [regex]::Escape('$ROOT')
    Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
      Where-Object { \$_.Name -eq 'tail.exe' -and (\$_.CommandLine -match \$root -or \$_.CommandLine -match \$rootUnix) } |
      ForEach-Object { Stop-Process -Id \$_.ProcessId -Force -ErrorAction SilentlyContinue }
  " &>/dev/null 2>&1 || true

  # Clean up only this project's orphaned Next.js frontend processes.
  powershell.exe -NoProfile -Command "
    function Stop-Tree([int]\$id) {
      Get-CimInstance Win32_Process -Filter \"ParentProcessId=\$id\" -ErrorAction SilentlyContinue |
        ForEach-Object { Stop-Tree ([int]\$_.ProcessId) }
      Stop-Process -Id \$id -Force -ErrorAction SilentlyContinue
    }
    \$client = [regex]::Escape('$CLIENTAPP_WIN')
    Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
      Where-Object { \$_.Name -in @('node.exe','cmd.exe') -and \$_.CommandLine -match \$client } |
      ForEach-Object { Stop-Tree ([int]\$_.ProcessId) }
  " &>/dev/null 2>&1 || true

  sleep 1
  ok "Service cleanup done"
}

stop_existing

# ─────────────────────────────────────────────
section "Build"
# ─────────────────────────────────────────────

info "Restoring NuGet packages..."
dotnet restore "$ROOT/MovieNight.sln" -v:quiet
ok "Packages restored"

info "Building solution..."
dotnet build "$ROOT/MovieNight.sln" --no-restore -c Debug -v:quiet /nodeReuse:false
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
  local project_path_win
  local bat_file_win
  local log_file_win
  project_path_win="$(winpath "$project_path")"
  bat_file_win="$(winpath "$bat_file")"
  log_file_win="$(winpath "$log_file")"

  # Prefer the Windows apphost .exe; fall back to dotnet DLL only if needed.
  local exe_path
  local dll_path
  exe_path="$project_path/bin/Debug/net10.0/${name}.exe"
  if [ "$name" = "Gateway" ]; then
    exe_path="$project_path/bin/Debug/net10.0/MovieNight.Gateway.exe"
  fi
  dll_path=$(find "$project_path/bin/Debug/net10.0" -maxdepth 1 -name "*.dll" \
    ! -name "*.Views.dll" ! -name "*.resources.dll" 2>/dev/null | head -1)
  if [ ! -f "$exe_path" ] && [ -z "$dll_path" ]; then
    err "No executable found for $name in $project_path/bin/Debug/net10.0 — did the build succeed?"
  fi
  local run_command
  if [ -f "$exe_path" ]; then
    run_command="\"$(winpath "$exe_path")\""
  else
    run_command="dotnet \"$(winpath "$dll_path")\""
  fi

  # Write wrapper bat.
  cat > "$bat_file" <<BATEOF
@echo off
cd /d "$project_path_win"
set ASPNETCORE_ENVIRONMENT=Development
set DOTNET_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:$port
$run_command > "$log_file_win" 2>&1
BATEOF

  # Launch hidden window via PowerShell, capture PID of the cmd.exe process
  local pid
  pid=$(powershell.exe -NoProfile -Command "
    \$p = Start-Process -FilePath 'cmd.exe' \`
      -ArgumentList '/c', '\"$bat_file_win\"' \`
      -WindowStyle Hidden \`
      -PassThru
    \$p.Id
  " 2>/dev/null | tr -d '\r')

  echo "$pid" > "$pid_file"
  ok "Started $name on :$port (PID $pid)"
}

start_service "Auth.API"         "$ROOT/Services/Auth/Auth.API"                    7010
start_service "Users.API"        "$ROOT/Services/User/Users.API"                  7001
start_service "Media.API"        "$ROOT/Services/Media/Media.API"                 7002
start_service "Access.API"       "$ROOT/Services/Access/Access.API"               7003
start_service "Friends.API"      "$ROOT/Services/Friends/Friends.API"             7004
start_service "MoviePlayer.API"  "$ROOT/Services/MoviePlayer/MoviePlayer.API"     7005
start_service "People.API"       "$ROOT/Services/People/People.API"               7006
start_service "Bookmark.API"     "$ROOT/Services/Bookmark/Bookmark.API"           7007
start_service "MovieRatings.API" "$ROOT/Services/MovieRatings/MovieRatings.API"   7008
start_service "Review.API"       "$ROOT/Services/Review/Review.API"               7011
start_service "Messages.API"     "$ROOT/Services/Messages/Messages.API"           7020
start_service "Achievements.API" "$ROOT/Services/Achievements/Achievements.API"   5102
start_service "Gateway"          "$ROOT/MovieNight.Gateway"                        7000

# ─────────────────────────────────────────────
section "Starting frontend"
# ─────────────────────────────────────────────

FRONTEND_BAT="$RUNTIME_DIR/Frontend.bat"
FRONTEND_PID="$RUNTIME_DIR/Frontend.pid"
FRONTEND_LOG="$RUNTIME_DIR/Frontend.log"
CLIENTAPP_WIN="$(winpath "$CLIENTAPP")"
FRONTEND_BAT_WIN="$(winpath "$FRONTEND_BAT")"
FRONTEND_LOG_WIN="$(winpath "$FRONTEND_LOG")"

cat > "$FRONTEND_BAT" <<BATEOF
@echo off
cd /d "$CLIENTAPP_WIN"
npm run dev > "$FRONTEND_LOG_WIN" 2>&1
BATEOF

FRONTEND_PID_VAL=$(powershell.exe -NoProfile -Command "
  \$p = Start-Process -FilePath 'cmd.exe' \`
    -ArgumentList '/c', '\"$FRONTEND_BAT_WIN\"' \`
    -WindowStyle Hidden \`
    -PassThru
  \$p.Id
" 2>/dev/null | tr -d '\r')

echo "$FRONTEND_PID_VAL" > "$FRONTEND_PID"
ok "Started Frontend on :3000 (PID $FRONTEND_PID_VAL)"

# ─────────────────────────────────────────────
section "Live log stream  (Ctrl+C to stop all)"
# ─────────────────────────────────────────────

# One colour per service so you can scan the stream at a glance
declare -A _CLR=(
  ["Auth.API"]=$'\033[1;36m'
  ["Users.API"]=$'\033[1;33m'
  ["Media.API"]=$'\033[1;35m'
  ["Access.API"]=$'\033[1;32m'
  ["Friends.API"]=$'\033[1;34m'
  ["MoviePlayer.API"]=$'\033[0;35m'
  ["People.API"]=$'\033[0;33m'
  ["Bookmark.API"]=$'\033[0;32m'
  ["MovieRatings.API"]=$'\033[0;36m'
  ["Review.API"]=$'\033[0;34m'
  ["Messages.API"]=$'\033[1;31m'
  ["Achievements.API"]=$'\033[0;37m'
  ["Gateway"]=$'\033[1;37m'
  ["Frontend"]=$'\033[1;32m'
)

FOLLOW_PIDS=()

# ── Print URL table cleanly BEFORE log stream starts ──
echo ""
echo -e "${BOLD}${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "  ${BOLD}${GREEN}✓ MovieNight started!${NC}  Press ${YELLOW}Ctrl+C${NC} to stop everything."
echo -e "${BOLD}${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""
echo -e "  ${BOLD}${CYAN}┌─ Open in browser ──────────────────────────────────────────┐${NC}"
echo -e "  ${BOLD}${CYAN}│${NC}  ${BOLD}Frontend     →  http://localhost:3000${NC}"
echo -e "  ${BOLD}${CYAN}│${NC}  Gateway docs →  http://localhost:7000/scalar/v1"
echo -e "  ${BOLD}${CYAN}└────────────────────────────────────────────────────────────┘${NC}"
echo ""
echo -e "  ${BOLD}${CYAN}── API services (Scalar docs) ────────────────────────────────${NC}"
echo -e "  ${CYAN}Auth.API         ${NC}→  http://localhost:7010/scalar/v1"
echo -e "  ${CYAN}Users.API        ${NC}→  http://localhost:7001/scalar/v1"
echo -e "  ${CYAN}Media.API        ${NC}→  http://localhost:7002/scalar/v1"
echo -e "  ${CYAN}Access.API       ${NC}→  http://localhost:7003/scalar/v1"
echo -e "  ${CYAN}Friends.API      ${NC}→  http://localhost:7004/scalar/v1"
echo -e "  ${CYAN}MoviePlayer.API  ${NC}→  http://localhost:7005/scalar/v1"
echo -e "  ${CYAN}People.API       ${NC}→  http://localhost:7006/scalar/v1"
echo -e "  ${CYAN}Bookmark.API     ${NC}→  http://localhost:7007/scalar/v1"
echo -e "  ${CYAN}MovieRatings.API ${NC}→  http://localhost:7008/scalar/v1"
echo -e "  ${CYAN}Review.API       ${NC}→  http://localhost:7011/scalar/v1"
echo -e "  ${CYAN}Messages.API     ${NC}→  http://localhost:7020/scalar/v1"
echo -e "  ${CYAN}Achievements.API ${NC}→  http://localhost:5102/scalar/v1"
echo ""
echo -e "  ${BOLD}${CYAN}── Gateway API routes (/api/gw/* from browser) ───────────────${NC}"
echo -e "  ${GREEN}POST${NC} /api/gw/auth/login      ${GREEN}POST${NC} /api/gw/auth/register"
echo -e "  ${GREEN}GET${NC}  /api/gw/auth/me         ${GREEN}GET${NC}  /api/gw/users"
echo -e "  ${GREEN}GET${NC}  /api/gw/users/me/profile  ${YELLOW}PUT${NC} /api/gw/users/me/profile"
echo -e "  ${GREEN}GET${NC}  /api/gw/movies          ${GREEN}GET${NC}  /api/gw/movies/search"
echo -e "  ${GREEN}GET${NC}  /api/gw/movies/films    ${GREEN}GET${NC}  /api/gw/movies/anime"
echo -e "  ${GREEN}GET${NC}  /api/gw/movies/cartoons ${GREEN}GET${NC}  /api/gw/movies/serial"
echo -e "  ${GREEN}GET${NC}  /api/gw/people/{id}     ${GREEN}GET${NC}  /api/gw/people/search"
echo -e "  ${GREEN}GET${NC}  /api/gw/bookmarks       ${GREEN}GET${NC}  /api/gw/bookmarks/watched"
echo -e "  ${GREEN}GET${NC}  /api/gw/ratings/movies/{id}  ${YELLOW}PUT${NC} /api/gw/ratings/movies/{id}"
echo -e "  ${GREEN}GET${NC}  /api/gw/review/{filmId} ${GREEN}POST${NC} /api/gw/review"
echo -e "  ${GREEN}GET${NC}  /api/gw/friends/{id}    ${GREEN}POST${NC} /api/gw/friends"
echo -e "  ${GREEN}GET${NC}  /api/gw/messages        ${GREEN}POST${NC} /api/gw/messages/compose"
echo -e "  ${GREEN}GET${NC}  /api/gw/healthz"
echo ""
echo -e "  ${YELLOW}Note:${NC} services are starting in background — it takes ~30s for all to be ready."
echo -e "  ${YELLOW}Logs${NC} →  .runtime/*.log    ${YELLOW}Stop${NC} →  ./stop-local.sh or Ctrl+C"
echo ""
echo -e "${BOLD}${BLUE}── Live log stream ───────────────────────────────────────────────${NC}"
echo ""

# Tail a log file in the background, prefix every line with a coloured service tag
follow_log() {
  local name="$1" logfile="$2"
  local color="${_CLR[$name]:-$NC}"
  local padded; printf -v padded "%-18s" "$name"

  # Wait up to 30 s for the log file to appear
  local t=0
  while [ ! -f "$logfile" ] && [ $t -lt 30 ]; do sleep 1; t=$((t+1)); done

  tail -n 0 -f "$logfile" 2>/dev/null \
    | while IFS= read -r line; do
        echo -e "${color}[${padded}]${NC} $line"
      done &
  FOLLOW_PIDS+=($!)
}

follow_log "Auth.API"         "$RUNTIME_DIR/Auth.API.log"
follow_log "Users.API"        "$RUNTIME_DIR/Users.API.log"
follow_log "Media.API"        "$RUNTIME_DIR/Media.API.log"
follow_log "Access.API"       "$RUNTIME_DIR/Access.API.log"
follow_log "Friends.API"      "$RUNTIME_DIR/Friends.API.log"
follow_log "MoviePlayer.API"  "$RUNTIME_DIR/MoviePlayer.API.log"
follow_log "People.API"       "$RUNTIME_DIR/People.API.log"
follow_log "Bookmark.API"     "$RUNTIME_DIR/Bookmark.API.log"
follow_log "MovieRatings.API" "$RUNTIME_DIR/MovieRatings.API.log"
follow_log "Review.API"       "$RUNTIME_DIR/Review.API.log"
follow_log "Messages.API"     "$RUNTIME_DIR/Messages.API.log"
follow_log "Achievements.API" "$RUNTIME_DIR/Achievements.API.log"
follow_log "Gateway"          "$RUNTIME_DIR/Gateway.log"
follow_log "Frontend"         "$RUNTIME_DIR/Frontend.log"

# Ctrl+C / SIGTERM → stop all services cleanly
_cleanup() {
  echo ""
  section "Stopping all services…"
  for pid in "${FOLLOW_PIDS[@]}"; do kill "$pid" 2>/dev/null || true; done
  bash "$ROOT/stop-local.sh"
  exit 0
}
trap _cleanup INT TERM

# Keep the terminal alive while background services run
while true; do sleep 5; done
