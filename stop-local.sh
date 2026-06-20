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
      # Stop the cmd.exe wrapper and its dotnet child tree
      powershell.exe -NoProfile -Command "
        function Stop-Tree(\$id) {
          Get-WmiObject Win32_Process -Filter \"ParentProcessId=\$id\" |
            ForEach-Object { Stop-Tree \$_.ProcessId }
          try { Stop-Process -Id \$id -Force -ErrorAction SilentlyContinue } catch {}
        }
        Stop-Tree $pid
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

# ── Kill node/npm (frontend) by port 3000 ──
section "Stopping frontend"

info "Looking for process on port 3000..."
PORT3000=$(powershell.exe -NoProfile -Command "
  \$conn = Get-NetTCPConnection -LocalPort 3000 -ErrorAction SilentlyContinue
  if (\$conn) { \$conn[0].OwningProcess } else { '' }
" 2>/dev/null | tr -d '\r\n ' || true)

if [ -n "$PORT3000" ] && [ "$PORT3000" != "0" ]; then
  powershell.exe -NoProfile -Command "
    function Stop-Tree(\$id) {
      Get-WmiObject Win32_Process -Filter \"ParentProcessId=\$id\" |
        ForEach-Object { Stop-Tree \$_.ProcessId }
      try { Stop-Process -Id \$id -Force -ErrorAction SilentlyContinue } catch {}
    }
    Stop-Tree $PORT3000
  " &>/dev/null 2>&1 || true
  ok "Stopped frontend process tree (root PID $PORT3000)"
else
  info "No process found on port 3000"
fi

# ── Optionally stop Docker container ──
if $STOP_DOCKER; then
  section "Stopping SQL Server Docker container"
  if docker inspect "$CONTAINER" &>/dev/null; then
    docker stop "$CONTAINER" &>/dev/null && docker rm "$CONTAINER" &>/dev/null || true
    ok "Stopped and removed container '$CONTAINER'"
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
