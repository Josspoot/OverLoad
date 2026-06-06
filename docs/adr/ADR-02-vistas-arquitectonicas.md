# ADR-02: Vistas Arquitectónicas del Sistema OverLoad

| Campo | Valor |
| :--- | :--- |
| **Autor** | Josué Enmanuel Poot Mateo |
| **Fecha** | 05/06/2026 |
| **Estado** | Propuesto |

---

## Contexto

A medida que el proyecto **OverLoad** crece, se hace necesario documentar su arquitectura desde diferentes perspectivas para facilitar la comprensión del sistema tanto para el desarrollador como para cualquier colaborador futuro.

Se utiliza el modelo de **4 vistas arquitectónicas** para describir el sistema desde los ángulos que más interesan a cada tipo de stakeholder:

| Vista | Enfocada en | Audiencia |
| :--- | :--- | :--- |
| Lógica | Clases, módulos y responsabilidades | Desarrolladores |
| Física | Nodos de hardware y artefactos de software | Operaciones / DevOps |
| Despliegue | Capas y flujo de comunicación | Desarrolladores / Ops |
| Procesos | Flujos en tiempo de ejecución | Desarrolladores / QA |

---

## Decisión

Se documentan las cuatro vistas arquitectónicas del sistema usando diagramas **Mermaid**, los cuales se renderizan directamente en GitHub sin herramientas externas.

Cada vista refleja el estado actual del proyecto: una aplicación **ASP.NET Core MVC** con autenticación via **ASP.NET Identity**, persistencia en **SQLite** mediante **Entity Framework Core**, y almacenamiento temporal en memoria para los ejercicios del tracker.

---

## Consecuencias

* La documentación queda versionada junto con el código fuente.
* Los diagramas Mermaid son modificables en texto plano y no requieren exportar imágenes.
* Al evolucionar el sistema (e.g., migrar de lista en memoria a base de datos real), los diagramas deben actualizarse para mantener consistencia.

---

## Vista Lógica

Muestra las clases principales del sistema, sus responsabilidades y relaciones. El patrón MVC divide el sistema en tres capas: **Controladores** (lógica de solicitudes), **Modelos** (datos del dominio) y **Vistas** (presentación HTML).

```mermaid
classDiagram
    direction TB

    class HomeController {
        -List~Ejercicio~ _ejercicios
        -int _contadorId
        +Index() IActionResult
        +Privacy() IActionResult
        +Tracker() IActionResult
        +Crear(Ejercicio) IActionResult
        +NuevaCarga(int,int,int,double) IActionResult
        +Eliminar(int) IActionResult
        +Error() IActionResult
    }

    class TrackerController {
        -List~Ejercicio~ _ejercicios
        -int _contadorId
        +Index() IActionResult
        +Crear(Ejercicio) IActionResult
        +NuevaCarga(int,int,int) IActionResult
        +Eliminar(int) IActionResult
    }

    class LibreriaController {
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

    class ApplicationDbContext {
        <<IdentityDbContext>>
    }

    class IdentityUser {
        <<ASP.NET Identity>>
        +string UserName
        +string Email
        +string PasswordHash
    }

    HomeController "1" --> "*" Ejercicio : gestiona en memoria
    TrackerController "1" --> "*" Ejercicio : gestiona en memoria
    ApplicationDbContext --> IdentityUser : persiste
    HomeController ..> ApplicationDbContext : inyectado via DI
```

---

## Vista Física

Describe cómo los artefactos de software se distribuyen sobre los nodos de hardware. En el estado actual del proyecto, todo corre en una sola máquina de desarrollo.

```mermaid
graph TB
    subgraph Cliente["Dispositivo del Usuario"]
        Browser["Navegador Web\nChrome / Firefox / Safari"]
    end

    subgraph Servidor["Máquina de Desarrollo / Servidor"]
        subgraph DotNet[".NET 10 Runtime"]
            Kestrel["Kestrel HTTP Server\n(puerto 5001 / 7001)"]
            App["OverLoad.dll\nASP.NET Core MVC"]
            IdentityMW["ASP.NET Identity\nMiddleware"]
            EFCore["Entity Framework Core\nSQLite Provider"]
            InMemory["Lista en Memoria\nList&lt;Ejercicio&gt;"]
        end
        DB[("app.db\nSQLite")]
        Static["wwwroot/\nCSS, JS, imágenes"]
    end

    Browser -->|"HTTP / HTTPS"| Kestrel
    Kestrel --> App
    App --> IdentityMW
    App --> EFCore
    App --> InMemory
    EFCore --> DB
    App --> Static
```

---

## Vista de Despliegue

Muestra las capas lógicas del sistema y cómo se comunican entre sí a través de protocolos y puertos definidos.

```mermaid
graph LR
    subgraph Presentacion["Capa de Presentación"]
        Browser["Navegador Web\n(Cliente)"]
    end

    subgraph Aplicacion["Capa de Aplicación\nlocalhost"]
        MVC["ASP.NET Core MVC\nControllers + Views"]
        Auth["ASP.NET Identity\nAutenticación / Sesión"]
        EF["EF Core\nSQLite Provider"]
    end

    subgraph Datos["Capa de Datos"]
        SQLiteFile[("SQLite\napp.db")]
    end

    Browser -->|"HTTPS :7001\nHTTP :5001"| MVC
    MVC <--> Auth
    MVC --> EF
    EF <-->|"SQL"| SQLiteFile
```

---

## Vista de Procesos

Describe el comportamiento del sistema en tiempo de ejecución. El diagrama muestra el flujo principal: el usuario registra un ejercicio desde el Tracker, el sistema lo procesa y devuelve la vista actualizada.

```mermaid
sequenceDiagram
    actor Usuario
    participant Browser as Navegador
    participant Router as Middleware / Router
    participant HC as HomeController
    participant Memory as Lista en Memoria
    participant View as Vista Razor (Tracker.cshtml)

    Usuario->>Browser: Llena formulario y presiona "Añadir"
    Browser->>Router: POST /Home/Crear (form data)
    Router->>Router: Verifica autenticación (Identity)
    Router->>HC: Crear(Ejercicio nuevoEjercicio)
    HC->>Memory: Asigna Id = _contadorId++
    HC->>Memory: _ejercicios.Add(nuevoEjercicio)
    Memory-->>HC: Ejercicio guardado en lista
    HC-->>Browser: RedirectToAction("Tracker")

    Browser->>Router: GET /Home/Tracker
    Router->>HC: Tracker()
    HC->>Memory: Obtiene _ejercicios
    Memory-->>HC: List&lt;Ejercicio&gt;
    HC->>View: return View(ejercicios)
    View-->>Browser: HTML con tabla de ejercicios
    Browser-->>Usuario: Visualiza el ejercicio registrado
```

---
