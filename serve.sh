#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# serve.sh — Publica OverLoad en internet por un tunel de Cloudflare (gratis).
#
# La app Y la base de datos (app.db) se quedan 100% en ESTA computadora.
# Cloudflare solo reenvia el trafico; nadie ve tus datos, solo la app.
#
# Sirve la version PUBLICADA (dotnet publish) a proposito: asi se generan los
# CSS/JS precomprimidos (.br/.gz) que .NET 10 necesita para servir bien los
# estilos detras del tunel (con "dotnet run" el CSS llega vacio por el tunel).
#
# Auto-actualizacion: mientras corre, vigila tu codigo; al guardar un cambio
# re-publica y reinicia SOLO la app. El tunel sigue vivo, asi la URL NO cambia.
#
# Uso:   ./serve.sh          (Ctrl+C baja todo)
# ---------------------------------------------------------------------------
set -euo pipefail

cd "$(dirname "$0")"
PROJ_DIR="$(pwd)"

PORT=5107
APP_URL="http://localhost:${PORT}"
PUB_DIR="${PROJ_DIR}/bin/serve-publish"           # salida del publish (gitignored)
DB_PATH="${PROJ_DIR}/app.db"                        # tu base de datos real, local
CONN="DataSource=${DB_PATH};Cache=Shared"
TUNNEL_LOG="$(mktemp -t overload-tunnel)"

APP_PID=""
TUNNEL_PID=""

cleanup() {
  echo ""
  echo "Deteniendo OverLoad y el tunel..."
  [[ -n "$APP_PID" ]] && kill "$APP_PID" 2>/dev/null || true
  [[ -n "$TUNNEL_PID" ]] && kill "$TUNNEL_PID" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

publish() {
  dotnet publish OverLoad.csproj -c Release -o "$PUB_DIR" --nologo >/tmp/overload-publish.log 2>&1
}

start_app() {
  ( cd "$PUB_DIR" && \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS="$APP_URL" \
    ConnectionStrings__DefaultConnection="$CONN" \
    dotnet OverLoad.dll ) &
  APP_PID=$!
  until curl -s -o /dev/null "${APP_URL}/"; do sleep 1; done
}

stop_app() {
  [[ -n "$APP_PID" ]] && kill "$APP_PID" 2>/dev/null || true
  wait "$APP_PID" 2>/dev/null || true
  APP_PID=""
}

echo "==> Publicando OverLoad (genera CSS/JS comprimidos)..."
publish
echo "==> Iniciando la app en ${APP_URL} (base de datos: ${DB_PATH})..."
start_app
echo "    app lista."

echo "==> Abriendo el tunel publico de Cloudflare..."
cloudflared tunnel --url "$APP_URL" > "$TUNNEL_LOG" 2>&1 &
TUNNEL_PID=$!

PUB=""
for _ in $(seq 1 30); do
  PUB="$(grep -Eo 'https://[a-z0-9-]+\.trycloudflare\.com' "$TUNNEL_LOG" | head -1 || true)"
  [[ -n "$PUB" ]] && break
  sleep 1
done

echo ""
echo "============================================================"
if [[ -n "$PUB" ]]; then
  echo "  OverLoad esta EN LINEA (con estilos):"
  echo ""
  echo "     $PUB"
  echo ""
  echo "  Comparte ese enlace. Al guardar cambios en el codigo se"
  echo "  actualiza solo (misma URL). Deja esta ventana abierta;"
  echo "  Ctrl+C baja todo."
else
  echo "  No pude leer la URL publica. Revisa: $TUNNEL_LOG"
fi
echo "============================================================"

# --- Vigilancia de cambios: re-publica y reinicia la app (tunel intacto) -----
STAMP="${PUB_DIR}/.serve-stamp"
touch "$STAMP"
WATCH="Program.cs Controllers Views wwwroot Application Areas Data Infrastructure Models"
while true; do
  sleep 2
  # ¿algun archivo fuente mas nuevo que la ultima publicacion?
  if [[ -n "$(find $WATCH -type f -newer "$STAMP" 2>/dev/null | head -1)" ]]; then
    echo ""
    echo "==> Cambios detectados: republicando y reiniciando..."
    stop_app
    if publish; then
      start_app
      touch "$STAMP"
      echo "    actualizado. La URL sigue siendo la misma."
    else
      echo "    ERROR al compilar (ver /tmp/overload-publish.log). Reintento al proximo cambio."
      start_app   # vuelve a levantar la version anterior para no dejar el sitio caido
    fi
  fi
done
