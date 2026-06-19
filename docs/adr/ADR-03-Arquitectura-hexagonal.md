# ADR-03: Adopción de Arquitectura Hexagonal en OverLoad

| Campo  | Valor |
|--------|-------|
| Autor  | Josué Enmanuel Poot Mateo |
| Fecha  | 12/06/2026 |
| Estado | `Propuesto` |

---

## Contexto

**OverLoad** es una aplicación para registrar y dar seguimiento a entrenamientos de fuerza (ejercicios, series, repeticiones, peso y esfuerzo). Hoy está construida como una aplicación **ASP.NET Core MVC** con autenticación vía **ASP.NET Identity**, persistencia en **SQLite** mediante **Entity Framework Core**, y almacenamiento temporal de los ejercicios en una **lista en memoria** dentro de los controladores (ver ADR-02).

El problema es que la lógica del dominio (cómo se crea un ejercicio, cómo se calcula la carga, qué reglas aplican) está **mezclada dentro de los controladores y atada a la web y a EF Core**. Esto bloquea dos objetivos que quiero para el proyecto:

1. **Desplegar la app en distintos canales**: además del sitio web, quiero poder consumir la misma lógica desde un **cliente móvil** (app nativa o PWA) sin reescribir las reglas.
2. **Manejar los datos con flexibilidad**: quiero poder guardar los ejercicios en **archivos** (JSON/CSV) o en **base de datos** de forma intercambiable, sin que el cambio afecte a la lógica de negocio.

Condiciones y restricciones que influyeron en la decisión:

- Es un proyecto académico/personal con un solo desarrollador, por lo que la arquitectura no puede ser tan pesada que frene el avance.
- Ya conozco **C#, ASP.NET Core, EF Core e Inyección de Dependencias (DI)**, que son la base para implementar puertos y adaptadores.
- En clase se revisaron estilos arquitectónicos y vistas (modelo 4+1), lo que me dio el marco para razonar la separación por capas.
- El tiempo disponible es limitado, así que se busca una arquitectura que se pueda introducir de forma incremental sobre el código actual.

---

## Decisión

Adoptar la **Arquitectura Hexagonal (Puertos y Adaptadores)**, reorganizando el sistema en tres anillos:

- **Núcleo de dominio (Core)**: contiene las entidades (`Ejercicio`) y la lógica de negocio en **servicios de aplicación** (casos de uso como "registrar ejercicio", "registrar nueva carga", "listar ejercicios", "eliminar"). El núcleo **no conoce** ASP.NET, EF Core ni el sistema de archivos.
- **Puertos**: interfaces definidas por el núcleo.
  - *Puerto de entrada* (driving): `IEjercicioService`, lo que el mundo exterior puede pedirle al sistema.
  - *Puerto de salida* (driven): `IEjercicioRepository`, lo que el sistema necesita del exterior para persistir datos.
- **Adaptadores**: implementaciones concretas que se conectan a los puertos mediante **DI**.
  - *Adaptadores de entrada*: una **API REST** (Web API) y los **controladores MVC** que invocan los casos de uso.
  - *Adaptadores de salida*: un **adaptador de archivos (JSON)** y un **adaptador SQLite/EF Core**, ambos implementando `IEjercicioRepository` e intercambiables por configuración.

Para el objetivo de múltiples canales, el núcleo expondrá una **API REST** que será consumida tanto por la **web (MVC)** como por un **futuro cliente móvil** (MAUI / React Native / PWA), todos como adaptadores de entrada sobre la misma lógica.

### ¿Por qué?

La característica clave de la arquitectura hexagonal que resuelve mi problema es la **inversión de dependencias hacia el dominio**: el núcleo define interfaces (puertos) y los detalles externos (web, móvil, archivos, base de datos) dependen de ellas, no al revés.

- Como la lógica de negocio queda aislada detrás del puerto `IEjercicioService`, **puedo agregar un nuevo canal (móvil) creando solo un adaptador de entrada nuevo**, sin tocar ni duplicar las reglas del dominio. Eso es exactamente lo que necesito para desplegar en web y móvil a la vez.
- Como la persistencia queda detrás del puerto `IEjercicioRepository`, **cambiar de archivos JSON a SQLite (o tener ambos)** es solo cuestión de registrar otro adaptador en la DI. Eso resuelve mi necesidad de manejar los datos como archivos con facilidad, sin reescribir la lógica.
- Reutilizo lo que ya sé (DI de ASP.NET Core) para "enchufar" los adaptadores, por lo que la decisión es realista con mi tiempo y experiencia.

### Alternativas consideradas

| Alternativa | Por qué la descarté |
|-------------|---------------------|
| Mantener MVC clásico con lógica en los controladores (estado actual) | La lógica queda atada a la web y a EF Core. Agregar un cliente móvil obligaría a duplicar reglas, y cambiar la persistencia a archivos implicaría tocar los controladores. No cumple mis dos objetivos. |
| Arquitectura en capas tradicional (Presentación → Negocio → Datos) | Separa responsabilidades, pero la capa de negocio normalmente sigue dependiendo "hacia abajo" de la capa de datos concreta. La intercambiabilidad de persistencia (archivo vs SQLite) y de canal (web vs móvil) queda menos garantizada que con puertos explícitos. |
| Microservicios | Sobredimensionado para un proyecto de un solo desarrollador. Añade complejidad de despliegue, red y operación que no necesito ahora y que frenaría el avance académico. |
| Clean Architecture / Onion completa | Comparte la idea de núcleo aislado, pero con más anillos y reglas formales. Para el tamaño de OverLoad, hexagonal me da el mismo beneficio (puertos/adaptadores) con menos ceremonia. |

---

## Consecuencias

**Lo que gano:**

- *Técnica*: añadir un nuevo canal de entrada (API REST para móvil) o cambiar el almacenamiento (de SQLite a archivos JSON o viceversa) se vuelve **localizado y de bajo riesgo**, porque solo implico un adaptador nuevo contra un puerto ya existente; el dominio no se modifica.
- *Técnica adicional*: la lógica de negocio queda **probable de forma aislada** (tests unitarios sobre los casos de uso con un repositorio en memoria simulado), sin necesidad de levantar la base de datos ni el servidor web.
- *Proceso / equipo*: el trabajo se organiza por **fronteras claras** (dominio, puertos, adaptadores). Esto facilita avanzar de forma incremental y, si en el futuro colabora alguien más, cada quien puede trabajar un adaptador sin pisar el núcleo.

**Lo que sacrifico o asumo:**

- *Limitación técnica*: aparece **más estructura desde el inicio** (interfaces, capa de aplicación, mapeo entre modelos del dominio y entidades de EF/DTOs). Para una funcionalidad muy simple, escribir el caso de uso, el puerto y el adaptador es más trabajo que un controlador directo.
- *Deuda o riesgo*: si el proyecto crece, deberé **mantener la disciplina de no filtrar detalles de infraestructura al núcleo** (por ejemplo, no usar tipos de EF Core dentro del dominio). Además, la migración del código actual (lógica dentro de los controladores) hacia este esquema debe hacerse de forma ordenada para no introducir regresiones, y queda pendiente definir el contrato de la API REST y la autenticación (Identity) como adaptador.

---

## Diagrama

Estructura del sistema bajo la arquitectura hexagonal: el núcleo en el centro, los puertos como fronteras y los adaptadores conectándose desde afuera.

```mermaid
graph LR
    subgraph Entrada["Adaptadores de Entrada (Driving)"]
        Web["Web MVC<br/>(Controllers + Razor)"]
        Api["API REST<br/>(Web API)"]
        Movil["Cliente Movil<br/>(MAUI / PWA)"]
    end

    subgraph Core["Nucleo de Dominio (Core)"]
        PortIn["Puerto de Entrada<br/>IEjercicioService"]
        UseCases["Casos de Uso<br/>(Servicios de Aplicacion)"]
        Domain["Entidades de Dominio<br/>Ejercicio"]
        PortOut["Puerto de Salida<br/>IEjercicioRepository"]

        PortIn --> UseCases
        UseCases --> Domain
        UseCases --> PortOut
    end

    subgraph Salida["Adaptadores de Salida (Driven)"]
        JsonRepo["Adaptador Archivos<br/>JSON / CSV"]
        SqliteRepo["Adaptador SQLite<br/>EF Core"]
    end

    Web --> PortIn
    Api --> PortIn
    Movil -->|HTTP/JSON| Api

    PortOut --> JsonRepo
    PortOut --> SqliteRepo
```

---

## Implementación realizada (18/06/2026)

La decisión dejó de ser solo propuesta: el núcleo hexagonal y el adaptador de persistencia **SQLite** ya están implementados en el código. A continuación se documenta cómo quedó la estructura real, manteniendo todo lo descrito arriba.

### Mapeo de los anillos a carpetas y clases

| Anillo | Rol | Carpeta / Namespace | Tipo |
|--------|-----|---------------------|------|
| Núcleo | Puerto de entrada (driving) | `Application/Ports` | `IEjercicioService` |
| Núcleo | Puerto de salida (driven) | `Application/Ports` | `IEjercicioRepository` |
| Núcleo | Caso de uso / servicio de aplicación | `Application/Services` | `EjercicioService` |
| Núcleo | Entidad de dominio | `Models` (`OverLoad.Models`) | `Ejercicio` |
| Adaptador de salida | Persistencia SQLite con EF Core | `Infrastructure/Persistence` | `EfEjercicioRepository` |
| Adaptador de entrada | Controlador web MVC | `Controllers` | `HomeController` |
| Composición | Registro de puertos y adaptadores en DI | raíz | `Program.cs` |

### Decisiones concretas de la implementación

- La entidad `Ejercicio` se **mantuvo en el namespace `OverLoad.Models`** (no se movió a una carpeta `Domain`). El snapshot de Entity Framework Core referencia la entidad por su nombre completo, y moverla habría generado una migración espuria. La arquitectura hexagonal se cumple igualmente, ya que lo decisivo es la **dirección de las dependencias** (el adaptador depende del puerto, no al revés), no la ubicación física del archivo.
- El `HomeController` dejó de depender de `ApplicationDbContext` y ahora depende **solo del puerto `IEjercicioService`**, por lo que ya no conoce EF Core ni la base de datos.
- El `EfEjercicioRepository` implementa `IEjercicioRepository` usando `ApplicationDbContext` (SQLite). Es el único punto que conoce EF Core para los ejercicios.

### Conexión de puertos y adaptadores (DI en `Program.cs`)

```csharp
// Arquitectura hexagonal: se enchufan los adaptadores a los puertos vía DI.
builder.Services.AddScoped<IEjercicioRepository, EfEjercicioRepository>();
builder.Services.AddScoped<IEjercicioService, EjercicioService>();
```

Cambiar de SQLite a un adaptador de archivos JSON sería, llegado el caso, sustituir únicamente la primera línea por la implementación correspondiente, sin tocar el núcleo ni el controlador.

### Estado de los objetivos del ADR

| Objetivo | Estado |
|----------|--------|
| Núcleo aislado tras puertos (entrada y salida) | Implementado |
| Adaptador de salida SQLite / EF Core | Implementado |
| Adaptador de entrada web (MVC) sobre el puerto | Implementado |
| Adaptador de salida de archivos (JSON/CSV) | Pendiente (previsto en este ADR) |
| Adaptador de entrada API REST + cliente móvil | Pendiente (previsto en este ADR) |

### Diagrama de clases de la implementación

Refleja los nombres reales de las clases e interfaces y la dirección de las dependencias hacia el núcleo.

```mermaid
classDiagram
    direction LR

    class HomeController {
        -IEjercicioService ejercicios
        +Tracker() Task~IActionResult~
        +Crear(Ejercicio) Task~IActionResult~
        +NuevaCarga(int,int,int,double) Task~IActionResult~
        +Eliminar(int) Task~IActionResult~
    }

    class IEjercicioService {
        <<puerto de entrada>>
        +ListarAsync() Task
        +RegistrarAsync(Ejercicio) Task
        +ActualizarCargaAsync(int,int,int,double) Task
        +EliminarAsync(int) Task
    }

    class EjercicioService {
        -IEjercicioRepository repositorio
    }

    class IEjercicioRepository {
        <<puerto de salida>>
        +ObtenerTodosAsync() Task
        +ObtenerPorIdAsync(int) Task
        +AgregarAsync(Ejercicio) Task
        +ActualizarAsync(Ejercicio) Task
        +EliminarAsync(int) Task
    }

    class EfEjercicioRepository {
        -ApplicationDbContext context
    }

    class Ejercicio {
        +int Id
        +string Nombre
        +string Enfoque
        +int Series
        +int Repeticiones
        +double Peso
        +int Esfuerzo
    }

    HomeController ..> IEjercicioService : depende del puerto
    EjercicioService ..|> IEjercicioService : implementa
    EjercicioService ..> IEjercicioRepository : depende del puerto
    EfEjercicioRepository ..|> IEjercicioRepository : implementa
    EfEjercicioRepository ..> Ejercicio : persiste
    EjercicioService ..> Ejercicio : opera
```
