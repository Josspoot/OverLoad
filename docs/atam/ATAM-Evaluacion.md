# Evaluación ATAM — OverLoad

| Campo | Valor |
| :--- | :--- |
| **Autor** | Josué Enmanuel Poot Mateo |
| **Fecha** | 30/07/2026 |
| **Método** | ATAM (Architecture Tradeoff Analysis Method) |
| **Estado** | Aceptado |

---

## 1. Objetivo

Evaluar la arquitectura de **OverLoad** con el método **ATAM** para exponer, sobre
**decisiones reales ya tomadas** (documentadas en los ADR-01 a ADR-07), al menos:

- un **punto de sensibilidad**,
- un **trade-off (compensación)**, y
- un **riesgo**,

cada uno justificado y ligado al atributo de calidad que impacta.

---

## 2. Atributos de calidad priorizados

Del contexto del proyecto (app académica individual de entrenamiento de fuerza) se
priorizan estos atributos, en este orden:

| # | Atributo | Por qué importa en OverLoad |
| :-: | :--- | :--- |
| 1 | **Mantenibilidad / Testabilidad** | Es un proyecto que evoluciona por unidades; debe poder crecer y probarse sin romperse. |
| 2 | **Disponibilidad** | La demo debe estar accesible y las funciones (incl. búsqueda de alimentos) deben responder. |
| 3 | **Escalabilidad / Concurrencia** | Varios usuarios podrían registrar entrenamientos al mismo tiempo. |
| 4 | **Seguridad / Portabilidad** | Manejo de identidad y de configuración/credenciales entre entornos. |
| 5 | **Costo** | Al ser estudiantil, la operación debe ser gratuita. |

---

## 3. Decisiones arquitectónicas evaluadas

| ID | Decisión | ADR |
| :-- | :--- | :-- |
| D1 | **Arquitectura hexagonal** (puertos y adaptadores) | ADR-03 |
| D2 | **Patrones GoF**: Strategy (progresión) y Decorator (logging del repositorio) | ADR-04 |
| D3 | **SQLite + EF Core** como persistencia única | ADR-01 / ADR-05 |
| D4 | **Cliente HTTP a Open Food Facts** (adaptador de salida) | ADR-05 |
| D5 | **Configuración en archivos versionados** (connection string en `appsettings.json`, parámetros del `HttpClient` fijos en `Program.cs`) | ADR-06 |
| D6 | **Hosting local por túnel** (app + `app.db` en la laptop, expuestos por Cloudflare Tunnel) | ADR-06 / `serve.sh` |
| D7 | **Suite xUnit + pipeline CI** | ADR-07 |

---

## 4. Punto de sensibilidad

> **Un punto de sensibilidad** es una decisión de la que **depende fuertemente**
> un atributo de calidad: mover esa sola decisión mueve el atributo de forma notable.

### PS-1 — La elección de **SQLite** es sensible a la **escalabilidad/concurrencia** (D3)

- **Descripción:** el motor de persistencia es un único parámetro del que depende
  directamente la concurrencia de escritura. SQLite serializa las escrituras
  (bloqueo a nivel de archivo/base). Mientras haya pocos usuarios simultáneos, el
  rendimiento es bueno; al aumentar la concurrencia de escritura, aparecen errores
  *"database is locked"* y la latencia se dispara. El atributo **cambia de forma
  abrupta** al variar **solo** esta decisión.
- **Justificación (decisión real):** ADR-01/ADR-05 eligieron SQLite por simplicidad
  y cero configuración para un proyecto individual.
- **Por qué es aislable:** gracias a la **hexagonal (D1)** y a EF Core, la persistencia
  está detrás del puerto `IEjercicioRepository` (y demás repos). Sustituir SQLite por
  PostgreSQL/SQL Server toca **solo el adaptador** y la cadena de conexión, sin afectar
  el núcleo ni los controladores. Eso confirma que es un punto de sensibilidad *localizado*.

---

## 5. Trade-off (compensación)

> **Un trade-off** es una decisión que es punto de sensibilidad para **dos o más
> atributos a la vez**, y que los mueve en **direcciones opuestas** (mejorar uno
> empeora otro).

### TO-1 — Hexagonal + patrones GoF: **mantenibilidad/testabilidad ↑** vs **simplicidad/esfuerzo ↓** (D1, D2)

- **Lado positivo:** los puertos y adaptadores, el **Strategy** de progresión
  (agregar un algoritmo = registrar una clase, sin tocar el selector) y el
  **Decorator** de logging (transparente, se quita comentando dos líneas) elevan la
  **mantenibilidad, la flexibilidad y la testabilidad** — de hecho las 5 clases de
  dominio puro se prueban sin base de datos ni red (ADR-07).
- **Lado negativo:** ese mismo diseño añade **más clases, más indirección y una
  curva de comprensión mayor**, y cuesta más tiempo de desarrollo inicial que un
  enfoque directo (controlador → EF). Es decir, se **sacrifica simplicidad y
  esfuerzo** para ganar mantenibilidad.
- **Por qué es un trade-off real:** optimizar la mantenibilidad/testabilidad (D1, D2)
  **degrada** la simplicidad y el costo de desarrollo; no se pueden maximizar ambos.
  La decisión (ADR-03, ADR-04) aceptó conscientemente ese costo porque el proyecto
  crece por unidades y la evolución pesa más que la rapidez inicial.

*(Trade-off secundario — D6: el hosting por túnel en la laptop maximiza el **costo
cero** a cambio de **disponibilidad**: el sitio solo existe con la laptop encendida.)*

---

## 6. Riesgos

> **Un riesgo** es una decisión que **puede impedir alcanzar** un atributo de calidad
> deseado.

### R-1 — Persistencia única en SQLite local + hosting en la laptop = **punto único de fallo** (D3, D6)

- **Atributo en peligro:** **disponibilidad y durabilidad de los datos**.
- **Descripción:** todos los datos viven en un solo archivo (`app.db`) en la laptop,
  sin réplica ni respaldo en la nube, y el servicio se sirve desde esa misma máquina
  por un túnel *best-effort*. Si falla el disco/equipo o se pierde el archivo, se
  **pierden todos los datos**; si la laptop se apaga o pierde red, el sitio **se cae**.
- **Decisión real que lo origina:** ADR-01/ADR-05 (SQLite) + la decisión de hosting
  local (`serve.sh` + Cloudflare Tunnel, ver ADR-06).
- **Mitigación propuesta:** respaldos periódicos de `app.db` (o migrar a un motor
  gestionado con backups) y, para 24/7, mover el runtime a un servicio siempre encendido.

### R-2 — Dependencia de **Open Food Facts sin resiliencia ni caché** (D4, D5)

- **Atributo en peligro:** **disponibilidad** de la búsqueda de alimentos.
- **Descripción:** el adaptador `OpenFoodFactsClient` llama a un servicio externo con
  `Timeout` y `User-Agent` **fijos en código** (ADR-06, deuda #1) y **sin caché,
  reintentos ni circuit-breaker**. Si Open Food Facts se cae, se degrada o cambia su
  contrato, la función de búsqueda **deja de responder** y no hay plan B.
- **Decisión real que lo origina:** ADR-05 (integración) + ADR-06 (parámetros
  hardcodeados como deuda técnica).
- **Mitigación propuesta:** patrón *Options* para la configuración, **caché** de
  resultados frecuentes y políticas de **reintento/circuit-breaker** (p. ej. Polly).

### R-3 — **Configuración/credenciales en archivos versionados** (D5)

- **Atributo en peligro:** **seguridad y portabilidad**.
- **Descripción:** la cadena de conexión está en `appsettings.json` versionado
  (ADR-06, deuda #1). Hoy es SQLite sin secreto, pero el patrón **no escala** con
  seguridad a un motor con credenciales, y mezcla configuración de entorno con el
  código.
- **Mitigación propuesta:** patrón *Options* + *user-secrets*/variables de entorno
  (`ConnectionStrings__DefaultConnection`), tal como propone el ADR-06.

---

## 7. Resumen

| Tipo | ID | Decisión (ADR) | Atributo impactado |
| :--- | :-- | :--- | :--- |
| **Punto de sensibilidad** | PS-1 | SQLite como persistencia (ADR-01/05) | Escalabilidad / concurrencia |
| **Trade-off** | TO-1 | Hexagonal + GoF (ADR-03/04) | Mantenibilidad/Testabilidad ↔ Simplicidad/Costo |
| **Riesgo** | R-1 | SQLite local + hosting por túnel (ADR-01/05/06) | Disponibilidad / durabilidad |
| **Riesgo** | R-2 | Open Food Facts sin resiliencia (ADR-05/06) | Disponibilidad |
| **Riesgo** | R-3 | Config en archivos versionados (ADR-06) | Seguridad / portabilidad |

La arquitectura está **bien posicionada para la mantenibilidad y la evolución** (su
atributo prioritario): la hexagonal aísla los puntos sensibles (persistencia,
servicios externos) detrás de puertos, de modo que los riesgos identificados son
**abordables sin rediseño**, con las mitigaciones ya propuestas en los ADR.

---

## 8. Declaración de uso de IA

Para esta evaluación ATAM se utilizó **Claude Code (Anthropic)** como asistente. El
uso concreto fue:

- **Apoyo de método:** ayudar a distinguir con rigor *punto de sensibilidad*,
  *trade-off* y *riesgo* según ATAM, y a mapearlos a decisiones reales del proyecto.
- **Análisis del código y de los ADR:** revisar las decisiones (hexagonal, GoF,
  SQLite, Open Food Facts, configuración, hosting) para sustentar cada punto.
- **Redacción y estructura** de este documento.

La priorización de atributos, la validación de que cada punto corresponde a una
decisión real y las conclusiones son responsabilidad del autor. La arquitectura y sus
decisiones son trabajo propio; la IA fue una herramienta de apoyo, no la autora.
