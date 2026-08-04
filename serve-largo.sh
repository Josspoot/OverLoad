#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# serve-largo.sh — Mantiene OverLoad EN LÍNEA de forma continua (varios días).
#
# Pensado para dejarlo corriendo, p. ej. 4 días seguidos. La app y la base de
# datos (app.db) siguen 100% en esta computadora, expuestas por Cloudflare Tunnel.
#
# Qué hace de más que serve.sh:
#   - caffeinate: evita que la Mac se DUERMA mientras este script viva.
#   - Supervisor: si la app o el túnel se caen, los REINICIA solos.
#   - Deja la URL pública vigente en el archivo URL-publica.txt.
#
# ⚠️ REQUISITOS FÍSICOS (obligatorios para que dure días):
#   - Mac ENCHUFADA a la corriente todo el tiempo.
#   - TAPA ABIERTA (cerrar la tapa la duerme aunque uses caffeinate).
#   - Buena ventilación e internet estable.
#
# Uso:   ./serve-largo.sh          (Ctrl+C detiene todo de forma limpia)
# ---------------------------------------------------------------------------
# Sin 'set -e' a propósito: el supervisor NO debe morir por un fallo puntual.
set -uo pipefail

cd "$(dirname "$0")"
PROJ_DIR="$(pwd)"

PORT=5107
APP_URL="http://localhost:${PORT}"
PUB_DIR="${PROJ_DIR}/bin/serve-publish"
DB_PATH="${PROJ_DIR}/app.db"
CONN="DataSource=${DB_PATH};Cache=Shared"
URL_FILE="${PROJ_DIR}/URL-publica.txt"
TUNNEL_LOG="$(mktemp -t overload-tunnel-largo)"

APP_PID=""
TUNNEL_PID=""
CAFF_PID=""

log() { printf '%s  %s\n' "$(date '+%Y-%m-%d %H:%M:%S')" "$*"; }

cleanup() {
  echo ""
  log "Deteniendo OverLoad, el túnel y caffeinate..."
  [[ -n "$TUNNEL_PID" ]] && kill "$TUNNEL_PID" 2>/dev/null
  [[ -n "$APP_PID" ]]    && kill "$APP_PID" 2>/dev/null
  [[ -n "$CAFF_PID" ]]   && kill "$CAFF_PID" 2>/dev/null
  pkill -f "dotnet OverLoad.dll" 2>/dev/null
}
trap cleanup EXIT INT TERM

# 1) Impedir que la Mac se duerma mientras este script (PID $$) siga vivo.
caffeinate -dimsu -w $$ &
CAFF_PID=$!
log "caffeinate activo: la Mac no se dormirá mientras corra este script."
log "Recuerda: enchufada y con la TAPA ABIERTA (cerrarla la duerme igual)."

publish() {
  log "Publicando (Release)..."
  if ! dotnet publish OverLoad.csproj -c Release -o "$PUB_DIR" --nologo >/tmp/overload-publish.log 2>&1; then
    log "ERROR al publicar. Revisa /tmp/overload-publish.log"
    return 1
  fi
}

app_alive()    { [[ -n "$APP_PID" ]]    && kill -0 "$APP_PID" 2>/dev/null; }
tunnel_alive() { [[ -n "$TUNNEL_PID" ]] && kill -0 "$TUNNEL_PID" 2>/dev/null; }

start_app() {
  ( cd "$PUB_DIR" && \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS="$APP_URL" \
    ConnectionStrings__DefaultConnection="$CONN" \
    dotnet OverLoad.dll ) >/tmp/overload-app.log 2>&1 &
  APP_PID=$!
  log "App iniciada (pid $APP_PID); esperando a que responda..."
  local ok=""
  for _ in $(seq 1 40); do
    if curl -s -o /dev/null "$APP_URL/"; then ok=1; break; fi
    sleep 1
  done
  [[ -n "$ok" ]] && log "App respondiendo en $APP_URL." || log "ADVERTENCIA: la app no respondió aún."
}

start_tunnel() {
  : > "$TUNNEL_LOG"
  cloudflared tunnel --url "$APP_URL" >"$TUNNEL_LOG" 2>&1 &
  TUNNEL_PID=$!
  local url=""
  for _ in $(seq 1 40); do
    url="$(grep -Eo 'https://[a-z0-9-]+\.trycloudflare\.com' "$TUNNEL_LOG" | head -1)"
    [[ -n "$url" ]] && break
    sleep 1
  done
  if [[ -n "$url" ]]; then
    echo "$url" > "$URL_FILE"
    log "TÚNEL LISTO. URL pública: $url"
    log "   (guardada en: $URL_FILE)"
  else
    log "ADVERTENCIA: no pude leer la URL del túnel. Revisa $TUNNEL_LOG"
  fi
}

# --- Arranque ---
publish || { log "No se pudo publicar; abortando."; exit 1; }
start_app
start_tunnel

echo ""
echo "============================================================"
echo "  OverLoad EN LÍNEA en modo continuo (supervisado)."
echo "  URL vigente -> $URL_FILE"
echo "  Déjalo corriendo los días que necesites. Ctrl+C detiene todo."
echo "============================================================"
echo ""

# --- Supervisor: revisa cada 10 s y revive lo que se haya caído ---
while true; do
  sleep 10
  if ! app_alive; then
    log "La app se cayó → reiniciando (la URL NO cambia)."
    start_app
  fi
  if ! tunnel_alive; then
    log "El túnel se cayó → reiniciando (⚠ la URL PÚBLICA CAMBIA; mira $URL_FILE)."
    start_tunnel
  fi
done
