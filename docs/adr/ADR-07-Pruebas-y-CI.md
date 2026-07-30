# ADR-07: Pruebas automatizadas e Integración Continua

| Campo | Valor |
| :--- | :--- |
| **Autor** | Josué Enmanuel Poot Mateo |
| **Fecha** | 15/07/2026 |
| **Estado** | Aceptado |

---

## Contexto

Hasta ahora OverLoad no tenía pruebas automatizadas: cada cambio se validaba a
mano abriendo la app. Eso no escala y no da garantía de que un refactor no rompa
las fórmulas del negocio. Se decide agregar una **suite de pruebas unitarias con
xUnit** y un **pipeline de Integración Continua (GitHub Actions)** que compile y
ejecute esas pruebas en cada `push` y cada Pull Request.

---

## Decisión

### Estructura

- Se crea un proyecto de pruebas independiente, **`OverLoad.Tests`** (SDK
  `Microsoft.NET.Sdk`, `net10.0`), que referencia al proyecto principal.
- Como el proyecto web hace *globbing* recursivo de `**/*.cs`, se excluye la
  carpeta `OverLoad.Tests/` de su compilación (`<Compile Remove="OverLoad.Tests/**" />`).
- Las pruebas siguen el patrón **Arrange–Act–Assert** y usan `[Fact]` y
  `[Theory]` con `[InlineData]` para los casos parametrizados.

### Qué clases se probaron y por qué

Se eligieron **clases del núcleo de dominio que son lógica pura**: no dependen de
EF Core, de la base de datos ni de ASP.NET, así que son deterministas, rápidas de
probar y son donde un error impacta directamente al usuario. Se cubren **cinco
clases** (la rúbrica pedía al menos tres):

| Clase | Archivo de prueba | Por qué se eligió |
| :--- | :--- | :--- |
| **`CalculadoraMetabolica`** | `CalculadoraMetabolicaTests.cs` | Concentra las fórmulas del negocio (TMB con Mifflin-St Jeor y Harris-Benedict, TDEE, ajuste por objetivo y macronutrientes). Un error aquí le daría al usuario objetivos calóricos incorrectos. Es el candidato más crítico y sin dependencias externas. |
| **`ProgresionPorPeso`** | `ProgresionEstrategiasTests.cs` | Estrategia de sobrecarga progresiva (patrón Strategy). Regla simple pero central: mantener series/reps y subir 2.5 kg. |
| **`DobleProgresion`** | `ProgresionEstrategiasTests.cs` | Estrategia con **bifurcación** (antes del tope suma repeticiones; al llegar al tope reinicia reps y sube peso). Las ramas condicionales son exactamente lo que conviene fijar con pruebas. |
| **`PoliticaProgresion`** | `PoliticaProgresionTests.cs` | Política de dominio con **aritmética de fechas** (recordatorio de progresión a 14 días). Los límites (justo en el día, antes, después) son fáciles de equivocar. |
| **`SelectorEstrategiaProgresion`** | `SelectorEstrategiaProgresionTests.cs` | "Contexto" del patrón Strategy: resuelve la estrategia por su clave. Debe ignorar mayúsculas y devolver `null` ante una clave inexistente; es el punto que desacopla el núcleo de las implementaciones. |

**Por qué NO se probaron (todavía) otras clases:** los repositorios EF Core
(`Ef*Repository`), los controladores y el cliente HTTP de Open Food Facts
dependen de infraestructura (base de datos, red, `HttpContext`). Probarlos exige
*mocks* o pruebas de integración; se dejan fuera de esta primera suite unitaria y
quedan como trabajo futuro.

### Pipeline de Integración Continua

- Workflow en `.github/workflows/ci.yml`, disparado en `push` y `pull_request`.
- Pasos: `checkout` → instalar SDK .NET 10 → `dotnet restore` → `dotnet build`
  (Release) → `dotnet test` (Release).
- Se probó que la compilación funciona **sin `app.db`** (ese archivo está
  gitignored y no existe en el runner), reproduciendo el entorno limpio del CI.

---

## Consecuencias y compensaciones

### Lo que gano

- **Red de seguridad:** cualquier cambio que rompa una fórmula del negocio hace
  fallar el CI antes de fusionar.
- **Documentación viva:** las pruebas describen con ejemplos concretos cómo debe
  comportarse cada regla (p. ej. que la doble progresión reinicia a 8 reps).
- **Proceso repetible:** el check verde en cada PR es evidencia objetiva de que
  el código compila y pasa las pruebas.

### Lo que asumo

- La suite es **unitaria**, no cubre integración con la base de datos ni la UI.
- Mantener las pruebas tiene un costo: hay que actualizarlas cuando cambien las
  reglas de negocio (es un costo deseado, no accidental).

---

## Declaración de uso de IA

Para esta entrega se utilizó **Claude Code (Anthropic)** como asistente. El uso
concreto fue:

- **Análisis del código:** la IA revisó las clases del proyecto para identificar
  cuáles son lógica pura y, por tanto, las mejores candidatas a pruebas
  unitarias.
- **Generación de las pruebas:** la IA ayudó a redactar los casos xUnit con
  estructura Arrange–Act–Assert, calculando los valores esperados de las fórmulas
  (TMB, TDEE, progresiones, fechas).
- **Configuración del pipeline:** la IA ayudó a escribir el workflow de GitHub
  Actions y a resolver el problema de *globbing* del SDK Web (exclusión de la
  carpeta de pruebas).
- **Redacción de este ADR.**

La ejecución de las pruebas, la verificación de que pasan en verde (local y en
CI) y la validación del contenido son responsabilidad del autor. El proyecto, su
arquitectura y las decisiones son trabajo propio; la IA fue una herramienta de
apoyo, no la autora.
