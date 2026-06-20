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
CONTAINER="movienight_mssql"
ROOT_WIN="$(cygpath -w "$ROOT")"
CLIENTAPP_WIN="$(cygpath -w "$ROOT/MovieNight.UI/clientapp")"

STOP_DOCKER=false
if [[ "${1:-}" == "--docker" || "${1:-}" == "-d" ]]; then
  STOP_DOCKER=true
fi

# ─────────────────────────────────────────────
section "Stopping MovieNight services"
# ─────────────────────────────────────────────

# ── Clear stale PID files ──
if [ -d "$RUNTIME_DIR" ] && ls "$RUNTIME_DIR"/*.pid &>/dev/null 2>&1; then
  info "Removing stale .pid files; processes are stopped by verified command-line scan below..."
  rm -f "$RUNTIME_DIR"/*.pid
  ok "Removed stale .pid files"
else
  info "No .pid files found in $RUNTIME_DIR"
fi

# ── Kill remaining service processes by command-line pattern ──
section "Cleaning up remaining service processes"

SERVICE_PATTERNS=(
  "Access.API"
  "Auth.API"
  "Achievements.API"
  "Bookmark.API"
  "Friends.API"
  "Media.API"
  "Messages.API"
  "MovieNight.Gateway"
  "MoviePlayer.API"
  "MovieRatings.API"
  "People.API"
  "Review.API"
  "Users.API"
)

info "Scanning for leftover MovieNight service processes..."
KILLED=$(powershell.exe -NoProfile -Command "
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
  \$procs = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
    Where-Object {
      \$_.CommandLine -match \$root -and (
        \$serviceNames -contains \$_.Name -or
        (\$_.Name -eq 'dotnet.exe' -and \$_.CommandLine -match \$servicePattern)
      )
    }
  foreach (\$proc in \$procs) {
    Stop-Process -Id \$proc.ProcessId -Force -ErrorAction SilentlyContinue
  }
  if (\$procs) { \$procs.Count } else { 0 }
" 2>/dev/null | tr -d '\r\n ' || echo "0")
if [ "$KILLED" -gt 0 ]; then
  echo -e "  ${GREEN}✓${NC} Killed $KILLED MovieNight service process(es)"
fi

if [ "$KILLED" -eq 0 ]; then
  ok "No leftover service processes found"
else
  ok "Killed $KILLED leftover process(es)"
fi

info "Scanning for leftover log watcher processes..."
WATCHERS_KILLED=$(powershell.exe -NoProfile -Command "
  \$root = [regex]::Escape('$ROOT_WIN')
  \$rootUnix = [regex]::Escape('$ROOT')
  \$procs = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
    Where-Object {
      (\$_.CommandLine -match \$root -or \$_.CommandLine -match \$rootUnix) -and (
        \$_.Name -eq 'tail.exe' -or
        (\$_.Name -in @('bash.exe','sh.exe') -and \$_.CommandLine -match 'start-local\.sh')
      )
    }
  foreach (\$proc in \$procs) {
    Stop-Process -Id \$proc.ProcessId -Force -ErrorAction SilentlyContinue
  }
  if (\$procs) { \$procs.Count } else { 0 }
" 2>/dev/null | tr -d '\r\n ' || echo "0")
if [ "$WATCHERS_KILLED" -gt 0 ]; then
  ok "Stopped $WATCHERS_KILLED leftover log watcher process(es)"
else
  ok "No leftover log watcher processes found"
fi

section "Stopping frontend"

FRONTEND_PID_FILE="$RUNTIME_DIR/Frontend.pid"
if [ -f "$FRONTEND_PID_FILE" ]; then
  FRONTEND_PID=$(cat "$FRONTEND_PID_FILE" 2>/dev/null | tr -d '[:space:]' || true)
  if [ -n "$FRONTEND_PID" ]; then
    powershell.exe -NoProfile -Command "
      function Stop-Tree([int]\$id) {
        Get-CimInstance Win32_Process -Filter \"ParentProcessId=\$id\" -ErrorAction SilentlyContinue |
          ForEach-Object { Stop-Tree ([int]\$_.ProcessId) }
        Stop-Process -Id \$id -Force -ErrorAction SilentlyContinue
      }
      \$proc = Get-CimInstance Win32_Process -Filter \"ProcessId=$FRONTEND_PID\" -ErrorAction SilentlyContinue
      \$bat = [regex]::Escape('$CLIENTAPP_WIN')
      if (\$proc -and (\$proc.CommandLine -match [regex]::Escape('Frontend.bat') -or \$proc.CommandLine -match \$bat)) {
        Stop-Tree $FRONTEND_PID
      }
    " &>/dev/null 2>&1 || true
    ok "Stopped frontend process (PID $FRONTEND_PID)"
  fi
else
  info "No Frontend.pid found"
fi

# ── Optionally stop Docker container ──
info "Scanning for leftover MovieNight frontend processes..."
FRONTEND_KILLED=$(powershell.exe -NoProfile -Command "
  function Stop-Tree([int]\$id) {
    Get-CimInstance Win32_Process -Filter \"ParentProcessId=\$id\" -ErrorAction SilentlyContinue |
      ForEach-Object { Stop-Tree ([int]\$_.ProcessId) }
    Stop-Process -Id \$id -Force -ErrorAction SilentlyContinue
  }
  \$client = [regex]::Escape('$CLIENTAPP_WIN')
  \$procs = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
    Where-Object { \$_.Name -in @('node.exe','cmd.exe') -and \$_.CommandLine -match \$client }
  foreach (\$proc in \$procs) { Stop-Tree ([int]\$proc.ProcessId) }
  if (\$procs) { \$procs.Count } else { 0 }
" 2>/dev/null | tr -d '\r\n ' || echo "0")
if [ "$FRONTEND_KILLED" -gt 0 ]; then
  ok "Stopped $FRONTEND_KILLED leftover frontend process tree(s)"
else
  ok "No leftover frontend processes found"
fi

if $STOP_DOCKER; then
  section "Stopping SQL Server Docker container"
  if docker inspect "$CONTAINER" &>/dev/null; then
    docker stop "$CONTAINER" &>/dev/null || true
    ok "Stopped container '$CONTAINER' (data preserved — run 'docker rm movienight_mssql' to delete)"
  else
    info "Container '$CONTAINER' not found — nothing to stop"
  fi
else
  echo ""
  echo -e "${YELLOW}→${NC} SQL Server container '${CONTAINER}' left running."
  echo -e "   To also stop it: ${BOLD}./stop-local.sh --docker${NC}"
fi

# ── Clean up .bat files ──
section "Cleanup"

if [ -d "$RUNTIME_DIR" ]; then
  rm -f "$RUNTIME_DIR"/*.pid
  rm -f "$RUNTIME_DIR"/*.bat
  ok "Removed .pid and .bat files from $RUNTIME_DIR"
fi

echo ""
ok "MovieNight stopped."
