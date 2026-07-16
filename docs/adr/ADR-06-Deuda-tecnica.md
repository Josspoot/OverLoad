# ADR-06: Deuda técnica identificada

| Campo | Valor |
| :--- | :--- |
| **Autor** | Josué Enmanuel Poot Mateo |
| **Fecha** | 15/07/2026 |
| **Estado** | Propuesto |

---

## Contexto

A lo largo del desarrollo de OverLoad se tomaron atajos conscientes para
cumplir con las fechas de entrega y, en otros casos, quedaron restos que no se
detectaron a tiempo tras refactores grandes (por ejemplo, el cambio del menú
de navegación de una hamburguesa desplegable a un rail siempre visible).

Este documento inventaría **dos deudas técnicas concretas y reales** del
proyecto, explica por qué existen, qué cuesta no pagarlas y qué técnica de
refactorización se aplicaría para saldarlas. No son problemas hipotéticos: cada
una apunta a archivos y líneas específicas del repositorio.

---

## Deuda técnica #1 — Configuración e infraestructura escritas a mano

### Qué es

Los parámetros de infraestructura del proyecto están incrustados directamente
en el código fuente y en configuración versionada, sin ninguna capa de
variables de entorno, *user-secrets* ni separación por ambiente:

1. **Cadena de conexión a la base de datos** en `appsettings.json` (archivo
   versionado en Git):

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "DataSource=app.db;Cache=Shared"
   }
   ```

2. **Parámetros del cliente HTTP de Open Food Facts** escritos como *magic
   strings* dentro de `Program.cs` (líneas del registro de `AddHttpClient`):

   ```csharp
   client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
   client.DefaultRequestHeaders.UserAgent.ParseAdd("OverLoad/1.0 (proyecto academico)");
   client.Timeout = TimeSpan.FromSeconds(15);
   ```

Ninguno de estos valores se lee desde `IConfiguration`, variables de entorno ni
el gestor de secretos de .NET. Están "quemados" en el binario o en un archivo
que se sube al repositorio.

### Por qué existe

Fue una **decisión consciente para avanzar rápido**. La aplicación es
académica, corre solo en `localhost` con SQLite y un único desarrollador, así
que hardcodear la ruta del archivo `app.db` y la URL de la API externa fue lo
más directo para tener algo funcionando y cumplir la fecha de entrega. No había
—en ese momento— un ambiente de producción real que obligara a separar la
configuración.

### Costo de no pagarla

Si la deuda crece (más ambientes, despliegue real, colaboradores o
credenciales reales):

- **No hay separación dev/prod:** llevar la app a un servidor exige **editar
  archivos versionados** y recompilar. Un cambio de configuración se convierte
  en un cambio de código.
- **Riesgo de fuga de credenciales:** hoy la cadena de conexión no tiene
  secretos, pero el patrón invita a que, cuando se migre a PostgreSQL/SQL Server
  o se agregue una API con *token*, alguien escriba usuario/contraseña o la
  *API key* directamente en `appsettings.json` y termine **publicada en el
  historial de Git** (imposible de borrar de forma limpia).
- **Fragilidad ante cambios externos:** si Open Food Facts cambia su dominio o
  se necesita apuntar a un *mock* en pruebas, hay que **recompilar** en lugar de
  cambiar una variable.
- **Pruebas acopladas:** no se puede inyectar una BD en memoria ni un endpoint
  falso por configuración, lo que endurece el testing de integración.

### Propuesta de solución

Aplicar **Extracción de configuración con el patrón Options** (refactor
*"Introduce Configuration Object" / "Replace Magic Literal with Configuration"*):

1. Mover la URL base, el *User-Agent* y el *timeout* de Open Food Facts a una
   sección de `appsettings.json`, y bindearlos con una clase de opciones:

   ```jsonc
   // appsettings.json
   "OpenFoodFacts": {
     "BaseUrl": "https://world.openfoodfacts.org/",
     "UserAgent": "OverLoad/1.0 (proyecto academico)",
     "TimeoutSeconds": 15
   }
   ```

   ```csharp
   builder.Services.Configure<OpenFoodFactsOptions>(
       builder.Configuration.GetSection("OpenFoodFacts"));
   ```

2. Para la cadena de conexión y cualquier secreto futuro, usar el gestor
   nativo de .NET en desarrollo y variables de entorno en despliegue:

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
   # o en el servidor:
   export ConnectionStrings__DefaultConnection="..."
   ```

   .NET ya combina estas fuentes automáticamente, así que **no hay que tocar
   código**: `GetConnectionString("DefaultConnection")` seguirá funcionando y
   tomará el valor del ambiente correcto.

3. Añadir un `appsettings.json` con valores *placeholder* (sin secretos) y
   documentar en el README las variables requeridas.

**Resultado:** la configuración deja de ser código; cambiar de ambiente o de
credencial ya no requiere recompilar ni arriesgar filtrar secretos en Git.
