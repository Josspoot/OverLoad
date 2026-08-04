<#
  serve-largo.ps1 — Mantiene OverLoad EN LINEA de forma continua en Windows 11.

  App + base de datos (app.db) 100% locales en esta laptop; expuesto por
  Cloudflare Tunnel (gratis). Equivalente Windows de serve-largo.sh (macOS).

  Que hace:
    - Impide que Windows se duerma mientras corre (SetThreadExecutionState).
    - Publica y sirve la version de Release (para que el CSS/JS carguen bien).
    - Usa la base de datos REAL por ruta absoluta (no crea una vacia).
    - Supervisa app y tunel: si alguno se cae, lo reinicia.
    - Deja la URL publica vigente en URL-publica.txt.

  REQUISITOS FISICOS (para que dure dias):
    - Laptop ENCHUFADA a la corriente.
    - TAPA ABIERTA (o configurar "Al cerrar la tapa: No hacer nada").
    - Internet estable.

  Como ejecutarlo (evita el bloqueo de scripts de PowerShell):
    powershell -ExecutionPolicy Bypass -File .\serve-largo.ps1

  Ctrl+C detiene todo de forma limpia.
#>

Set-Location -Path $PSScriptRoot

$Port    = 5107
$AppUrl  = "http://localhost:$Port"
$PubDir  = Join-Path $PSScriptRoot 'bin\serve-publish'
$DbPath  = Join-Path $PSScriptRoot 'app.db'
$Conn    = "DataSource=$DbPath;Cache=Shared"
$UrlFile = Join-Path $PSScriptRoot 'URL-publica.txt'
$TunLog  = Join-Path $env:TEMP 'overload-tunnel.log'
$TunErr  = Join-Path $env:TEMP 'overload-tunnel.err'
$AppLog  = Join-Path $env:TEMP 'overload-app.log'
$AppErr  = Join-Path $env:TEMP 'overload-app.err'

$global:AppProc = $null
$global:TunProc = $null

function Log([string]$msg) {
  Write-Host ("{0}  {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $msg)
}

# --- 1) Evitar que Windows se duerma mientras viva este script ---
Add-Type -Namespace Win32 -Name Power -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("kernel32.dll")]
public static extern uint SetThreadExecutionState(uint esFlags);
'@
# ES_CONTINUOUS(0x80000000)|ES_SYSTEM_REQUIRED(0x1)|ES_DISPLAY_REQUIRED(0x2)
[void][Win32.Power]::SetThreadExecutionState([uint32]'0x80000003')
Log "Modo 'no dormir' activado (mientras corra este script). Recuerda: enchufada y tapa abierta."

function Invoke-Publish {
  Log "Publicando (Release)..."
  & dotnet publish OverLoad.csproj -c Release -o $PubDir --nologo | Out-Null
  if ($LASTEXITCODE -ne 0) { throw "Error al publicar (revisa la salida de dotnet)." }
}

function Start-App {
  $env:ASPNETCORE_ENVIRONMENT = 'Production'
  $env:ASPNETCORE_URLS        = $AppUrl
  $env:ConnectionStrings__DefaultConnection = $Conn
  $global:AppProc = Start-Process -FilePath 'dotnet' -ArgumentList 'OverLoad.dll' `
      -WorkingDirectory $PubDir -NoNewWindow -PassThru `
      -RedirectStandardOutput $AppLog -RedirectStandardError $AppErr
  Log "App iniciada (PID $($global:AppProc.Id)); esperando a que responda..."
  for ($i = 0; $i -lt 40; $i++) {
    try { Invoke-WebRequest -UseBasicParsing -Uri "$AppUrl/" -TimeoutSec 3 | Out-Null; break }
    catch { Start-Sleep -Seconds 1 }
  }
  Log "App respondiendo en $AppUrl."
}

function Start-Tunnel {
  Remove-Item $TunLog, $TunErr -ErrorAction SilentlyContinue
  $global:TunProc = Start-Process -FilePath 'cloudflared' `
      -ArgumentList @('tunnel', '--url', $AppUrl) -NoNewWindow -PassThru `
      -RedirectStandardOutput $TunLog -RedirectStandardError $TunErr
  $url = $null
  for ($i = 0; $i -lt 40; $i++) {
    Start-Sleep -Seconds 1
    # cloudflared imprime la URL normalmente por STDERR.
    $hit = Select-String -Path $TunErr, $TunLog -Pattern 'https://[a-z0-9-]+\.trycloudflare\.com' `
             -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($hit) { $url = $hit.Matches[0].Value; break }
  }
  if ($url) {
    $url | Set-Content -Path $UrlFile -Encoding UTF8
    Log "TUNEL LISTO. URL publica: $url"
    Log "   (guardada en: $UrlFile)"
  } else {
    Log "ADVERTENCIA: no pude leer la URL del tunel. Revisa $TunErr"
  }
}

try {
  Invoke-Publish
  Start-App
  Start-Tunnel

  Write-Host ""
  Write-Host "============================================================"
  Write-Host "  OverLoad EN LINEA en modo continuo (supervisado)."
  Write-Host "  URL vigente ->  $UrlFile"
  Write-Host "  Dejalo corriendo los dias que necesites. Ctrl+C detiene todo."
  Write-Host "============================================================"
  Write-Host ""

  # --- Supervisor: revisa cada 10 s y revive lo que se haya caido ---
  while ($true) {
    Start-Sleep -Seconds 10
    if ($global:AppProc.HasExited) {
      Log "La app se cayo -> reiniciando (la URL NO cambia)."
      Start-App
    }
    if ($global:TunProc.HasExited) {
      Log "El tunel se cayo -> reiniciando (la URL PUBLICA CAMBIA; mira $UrlFile)."
      Start-Tunnel
    }
  }
}
finally {
  Write-Host ""
  Log "Deteniendo OverLoad y el tunel..."
  if ($global:TunProc -and -not $global:TunProc.HasExited) { $global:TunProc.Kill() }
  if ($global:AppProc -and -not $global:AppProc.HasExited) { $global:AppProc.Kill() }
  # Libera el bloqueo de suspension (solo ES_CONTINUOUS).
  [void][Win32.Power]::SetThreadExecutionState([uint32]'0x80000000')
}
