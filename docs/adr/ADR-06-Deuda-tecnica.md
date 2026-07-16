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

---

## Deuda técnica #2 — Código muerto y comentarios engañosos tras el refactor del menú

### Qué es

Cuando el menú de navegación pasó de ser una **hamburguesa desplegable** a un
**rail lateral siempre visible que se expande al clic**, se cambió el marcado y
el JavaScript, pero **quedaron restos del diseño anterior**:

1. **CSS muerto** en `wwwroot/css/site.css`: las reglas `.nav-hamburger` y
   `.hamburger-bars` (aprox. líneas **1022–1059**), incluidos sus
   pseudo-elementos `::before`/`::after`, ya no tienen ningún elemento que las
   use — el botón de la hamburguesa se eliminó del `_Layout.cshtml`.
2. **Comentario engañoso** en `Views/Shared/_Layout.cshtml` línea **49**:

   ```html
   @* ===== Menú de navegación lateral (oculto, se abre con la hamburguesa) ===== *@
   ```

   El menú **ya no está oculto** ni se abre con una hamburguesa: está siempre
   visible. El comentario miente sobre el comportamiento actual.
3. **Alias CSS de compatibilidad** en `:root` (site.css líneas 52–56):
   `--accent-red`, `--text-white`, `--text-light`, `--bg-dark`, `--bg-black`.
   Se crearon como puente para que los estilos *inline* viejos que aún usan
   `var(--accent-red)` no se rompieran al introducir los tokens semánticos.
   Estas variables *inline* siguen esparcidas en **5 vistas**
   (`Home/Index`, `Home/Tracker`, `Bitacora/Index`, `Bitacora/Buscar`,
   `Calculadora/Index`), duplicando el vocabulario de diseño.

### Por qué existe

Es **descuido no detectado a tiempo** combinado con un atajo consciente. Al
refactorizar el nav bajo presión de entrega, se priorizó que la nueva versión
funcionara y se dejó el código viejo "por si acaso", sin volver a limpiarlo.
Los alias `--accent-red` etc. fueron un atajo deliberado para **no tener que
editar las 5 vistas** en ese momento: era más rápido crear un alias que hacer
*buscar y reemplazar* en todos los archivos.

### Costo de no pagarla

- **Ventanas rotas:** el código muerto hace creer que la hamburguesa todavía
  existe. Un futuro desarrollador (o yo mismo en unos meses) puede perder
  tiempo intentando "arreglar" o reutilizar reglas que no hacen nada.
- **Comentarios que mienten** son peor que no tener comentarios: inducen a
  decisiones equivocadas.
- **Doble vocabulario de diseño:** mientras existan `--accent-red` y `--accent`
  para lo mismo, cada estilo nuevo puede elegir el equivocado, y una futura
  limpieza de temas tendrá que rastrear ambos. Si alguien borra el alias sin
  buscar sus usos, **5 vistas pierden su color** silenciosamente (no hay error
  de compilación en CSS).
- El archivo `site.css` (1160 líneas) sigue creciendo con peso muerto, lo que
  ralentiza su lectura y mantenimiento.

### Propuesta de solución

Aplicar **Eliminación de código muerto** + **Reemplazo por búsqueda y
sustitución** (*refactor "Remove Dead Code" + "Rename/Consolidate"*):

1. **Borrar** las reglas `.nav-hamburger` y `.hamburger-bars` (y sus
   pseudo-elementos) de `site.css`. Verificar con una búsqueda global que
   ningún marcado las referencia antes de eliminar.
2. **Corregir el comentario** de `_Layout.cshtml:49` para que describa el
   comportamiento real ("rail lateral siempre visible que se expande al clic").
3. **Consolidar el vocabulario de diseño:** hacer *buscar y reemplazar* de
   `var(--accent-red)` → `var(--accent)`, `var(--text-white)` → `var(--heading)`,
   etc. en las 5 vistas, y **luego eliminar los alias** de `:root`. Con esto
   queda un único conjunto de *tokens* semánticos.
4. (Opcional) Añadir estos pasos como *checklist* de "limpieza post-refactor"
   para no volver a dejar restos.

**Resultado:** el CSS refleja solo lo que la app hace hoy, los comentarios
dicen la verdad y hay una sola fuente de verdad para los colores del tema.

---

## Consecuencias y compensaciones

### Lo que gano al documentar esta deuda

- **Visibilidad:** la deuda deja de ser conocimiento tácito en la cabeza del
  autor y queda registrada con ubicación exacta y plan de pago.
- **Priorización informada:** se puede decidir *cuándo* pagar cada deuda con
  datos (costo vs. esfuerzo) en lugar de descubrirla en el peor momento.

### Lo que asumo

- Documentar no es pagar: las deudas siguen presentes hasta ejecutar los
  refactores. Este ADR es el compromiso de saldarlas, no el saldo.
- La Deuda #1 debe atenderse **antes** de cualquier despliegue real o de
  introducir credenciales verdaderas; la Deuda #2 es de menor riesgo y puede
  pagarse de forma incremental.

---

## Declaración de uso de IA

Para la elaboración de este ADR se utilizó **Claude Code (Anthropic)** como
asistente. El uso concreto fue:

- **Análisis del repositorio:** la IA inspeccionó `appsettings.json`,
  `Program.cs`, `wwwroot/css/site.css` y `Views/Shared/_Layout.cshtml` para
  identificar deuda técnica **real y específica** (ubicaciones y líneas
  concretas), en lugar de ejemplos genéricos.
- **Redacción:** la IA ayudó a estructurar y redactar el documento siguiendo el
  formato de los ADR previos del proyecto (ADR-01 a ADR-05).

La **verificación de que las deudas son reales**, la decisión de cuáles
documentar y la validación del contenido son responsabilidad del autor. El
código, las decisiones de arquitectura y este repositorio son trabajo propio;
la IA fue una herramienta de apoyo para análisis y redacción, no la autora del
proyecto.
