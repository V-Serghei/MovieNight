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
CLIENTAPP_WIN="$(cygpath -w "$ROOT/MovieNight.UI/clientapp")"

STOP_DOCKER=false
if [[ "${1:-}" == "--docker" || "${1:-}" == "-d" ]]; then
  STOP_DOCKER=true
fi

# ─────────────────────────────────────────────
section "Stopping MovieNight services"
# ─────────────────────────────────────────────

# ── Kill by PID files ──
if [ -d "$RUNTIME_DIR" ] && ls "$RUNTIME_DIR"/*.pid &>/dev/null 2>&1; then
  info "Stopping processes from .pid files..."
  STOPPED=0
  for pidfile in "$RUNTIME_DIR"/*.pid; do
    name="$(basename "$pidfile" .pid)"
    pid=$(cat "$pidfile" 2>/dev/null | tr -d '[:space:]' || true)
    if [ -n "$pid" ]; then
      bat_win="$(cygpath -w "$RUNTIME_DIR/${name}.bat")"
      powershell.exe -NoProfile -Command "
        function Stop-Tree([int]\$id) {
          Get-CimInstance Win32_Process -Filter \"ParentProcessId=\$id\" -ErrorAction SilentlyContinue |
            ForEach-Object { Stop-Tree ([int]\$_.ProcessId) }
          Stop-Process -Id \$id -Force -ErrorAction SilentlyContinue
        }
        \$proc = Get-CimInstance Win32_Process -Filter \"ProcessId=$pid\" -ErrorAction SilentlyContinue
        if (\$proc -and \$proc.CommandLine -match [regex]::Escape('$bat_win')) {
          Stop-Tree $pid
        }
      " &>/dev/null 2>&1 || true
      STOPPED=$((STOPPED + 1))
      echo -e "  ${GREEN}✓${NC} Stopped $name (PID $pid)"
    fi
    rm -f "$pidfile"
  done
  ok "Stopped $STOPPED service(s) from PID files"
else
  info "No .pid files found in $RUNTIME_DIR"
fi

# ── Kill remaining dotnet processes by command-line pattern ──
section "Cleaning up remaining dotnet processes"

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

info "Scanning for leftover dotnet service processes..."
KILLED=0
for pattern in "${SERVICE_PATTERNS[@]}"; do
  count=$(powershell.exe -NoProfile -Command "
    \$procs = Get-WmiObject Win32_Process -Filter \"Name='dotnet.exe'\" |
      Where-Object { \$_.CommandLine -match [regex]::Escape('$pattern') }
    if (\$procs) {
      \$procs | ForEach-Object { Stop-Process -Id \$_.ProcessId -Force -ErrorAction SilentlyContinue }
      \$procs.Count
    } else { 0 }
  " 2>/dev/null | tr -d '\r\n ' || echo "0")
  if [ "$count" -gt 0 ]; then
    echo -e "  ${GREEN}✓${NC} Killed $count process(es) matching '$pattern'"
    KILLED=$((KILLED + count))
  fi
done

if [ "$KILLED" -eq 0 ]; then
  ok "No leftover dotnet processes found"
else
  ok "Killed $KILLED leftover process(es)"
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
      if (\$proc -and \$proc.CommandLine -match [regex]::Escape('Frontend.bat')) {
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
