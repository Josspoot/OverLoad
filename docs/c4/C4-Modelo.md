# Modelo C4 — OverLoad

Documentación de la arquitectura de **OverLoad** (app de seguimiento de entrenamientos de fuerza)
siguiendo el [Modelo C4](https://c4model.com/) de Simon Brown. Los diagramas están escritos como
código **Mermaid** para que vivan versionados junto al código fuente y evolucionen con él.

| Campo  | Valor |
|--------|-------|
| Autor  | Josué Enmanuel Poot Mateo |
| Proyecto | OverLoad |
| Niveles | 1. Contexto · 2. Contenedores · 3. Componentes |
| Notación | Mermaid (C4) |

> Cada nivel hace **zoom** sobre el anterior: del sistema completo (Nivel 1), a sus piezas
> técnicas desplegables (Nivel 2), al interior de la pieza principal (Nivel 3).

---

## C4 Nivel 1 — Diagrama de Contexto

**¿Para quién es?** Cualquier persona (usuarios, docentes, evaluadores) que quiera entender
**qué es OverLoad y quién lo usa**, sin detalle técnico.

**¿Qué pregunta responde?** *¿Quién interactúa con el sistema y con qué sistemas externos se relaciona?*

```mermaid
flowchart TB
    atleta["<b>Atleta / Usuario</b><br/><i>[Persona]</i><br/>Registra sus entrenamientos, consulta la librería de ejercicios y usa la calculadora metabólica."]
    clienteApi["<b>Consumidor de la API</b><br/><i>[Persona / Sistema]</i><br/>App móvil o cliente externo que consume la API REST."]
    overload["<b>OverLoad</b><br/><i>[Sistema de software]</i><br/>Aplicación web para registrar y dar seguimiento a entrenamientos de fuerza: progresión de cargas, librería de ejercicios y calculadora metabólica (TMB/TDEE)."]
    identity["<b>ASP.NET Identity</b><br/><i>[Sistema externo]</i><br/>Gestión de cuentas, registro e inicio de sesión."]

    atleta -->|"Registra entrenamientos y consulta<br/>HTTPS / navegador"| overload
    clienteApi -->|"Consume casos de uso y sugerencias<br/>HTTPS / JSON (REST)"| overload
    overload -->|"Autentica y autoriza usuarios<br/>ASP.NET Core Identity"| identity

    classDef persona fill:#08427b,stroke:#052e56,color:#ffffff;
    classDef sistema fill:#1168bd,stroke:#0b4884,color:#ffffff;
    classDef externo fill:#999999,stroke:#6b6b6b,color:#ffffff;
    class atleta,clienteApi persona
    class overload sistema
    class identity externo
```

**Notas del nivel**
- El **Atleta** es el usuario principal: usa la interfaz web (MVC + Razor).
- El **Consumidor de la API** representa un canal alternativo (ej. app móvil) previsto por la
  arquitectura hexagonal; consume la misma lógica de negocio vía REST.
- **ASP.NET Identity** se modela como sistema externo porque provee autenticación lista para usar,
  fuera del dominio propio de OverLoad.

---

## C4 Nivel 2 — Diagrama de Contenedores

**¿Para quién es?** Desarrolladores y evaluadores técnicos que quieren ver **las piezas grandes
desplegables** del sistema y cómo se comunican.

**¿Qué pregunta responde?** *¿De qué bloques técnicos se compone OverLoad y qué tecnología usa cada uno?*

```mermaid
flowchart TB
    atleta["<b>Atleta / Usuario</b><br/><i>[Persona]</i>"]
    clienteApi["<b>Consumidor de la API</b><br/><i>[Persona / Sistema]</i>"]
    identity["<b>ASP.NET Identity</b><br/><i>[Sistema externo]</i><br/>Autenticación y gestión de cuentas."]

    subgraph overload["Sistema OverLoad"]
        direction TB
        web["<b>Aplicación Web MVC</b><br/><i>[ASP.NET Core MVC + Razor]</i><br/>Inicio, tracker de ejercicios, librería y calculadora. Adaptador de entrada."]
        api["<b>API REST</b><br/><i>[ASP.NET Core Web API + Swagger]</i><br/>Casos de uso y sugerencias de progresión en JSON. Adaptador de entrada."]
        nucleo["<b>Núcleo de Aplicación</b><br/><i>[C# / .NET]</i><br/>Casos de uso, puertos, Strategy y servicios de dominio. No conoce web ni base de datos."]
        persistencia["<b>Adaptador de Persistencia</b><br/><i>[EF Core + Decorator]</i><br/>Implementa IEjercicioRepository; envuelto por un decorador de logging."]
        db[("<b>Base de datos</b><br/><i>[SQLite]</i><br/>Ejercicios, Identity y migraciones.")]
    end

    atleta -->|"Usa · HTTPS"| web
    clienteApi -->|"Consume · HTTPS / JSON"| api
    web -->|"Invoca casos de uso<br/>IEjercicioService"| nucleo
    api -->|"Invoca casos de uso<br/>IEjercicioService"| nucleo
    nucleo -->|"Persiste / consulta<br/>IEjercicioRepository"| persistencia
    persistencia -->|"Lee / escribe<br/>EF Core / SQL"| db
    web -->|"Autentica<br/>ASP.NET Identity"| identity

    classDef persona fill:#08427b,stroke:#052e56,color:#ffffff;
    classDef externo fill:#999999,stroke:#6b6b6b,color:#ffffff;
    classDef contenedor fill:#438dd5,stroke:#2e6295,color:#ffffff;
    class atleta,clienteApi persona
    class identity externo
    class web,api,nucleo,persistencia,db contenedor
```

**Notas del nivel**
- **Web MVC** y **API REST** son dos adaptadores de entrada distintos que reutilizan el **mismo
  núcleo** a través del puerto `IEjercicioService` — el beneficio clave de la arquitectura hexagonal.
- El **Núcleo** define los puertos pero no depende de EF Core ni de ASP.NET; los adaptadores se
  enchufan por inyección de dependencias.
- El **Adaptador de Persistencia** es en realidad un `EfEjercicioRepository` envuelto por un
  `EjercicioRepositoryLogDecorator` (Decorator), transparente para el núcleo.

---

## C4 Nivel 3 — Diagrama de Componentes

**¿Para quién es?** Desarrolladores que van a **modificar o extender el código** y necesitan saber
qué clases viven dentro de la pieza principal y cómo colaboran.

**¿Qué pregunta responde?** *¿Qué controladores, servicios y patrones GoF componen el núcleo de
OverLoad y cómo se conectan a través de los puertos?*

Se hace zoom sobre los adaptadores de entrada (controladores) y el **Núcleo de Aplicación**, hasta
el adaptador de salida y la base de datos.

```mermaid
flowchart TB
    atleta["<b>Atleta / Usuario</b><br/><i>[Persona]</i>"]
    clienteApi["<b>Consumidor de la API</b><br/><i>[Persona / Sistema]</i>"]

    subgraph entrada["Adaptadores de entrada (driving)"]
        direction LR
        homeCtrl["<b>HomeController</b><br/><i>[MVC]</i><br/>Tracker (CRUD) y librería."]
        calcCtrl["<b>CalculadoraController</b><br/><i>[MVC]</i><br/>TMB/TDEE, macros, estimaciones."]
        apiCtrl["<b>EjerciciosApiController</b><br/><i>[Web API]</i><br/>/api/v1/ejercicios."]
    end

    subgraph nucleo["Núcleo de Aplicación (hexágono)"]
        iservice["<b>IEjercicioService</b><br/><i>[Puerto de entrada]</i>"]
        service["<b>EjercicioService</b><br/><i>[Servicio de aplicación]</i>"]
        selector["<b>SelectorEstrategiaProgresion</b><br/><i>[Strategy · contexto]</i>"]
        strategies["<b>IEstrategiaProgresion + 4 estrategias</b><br/><i>[Strategy · comportamiento]</i>"]
        calc["<b>CalculadoraMetabolica</b><br/><i>[Servicio de dominio]</i>"]
        catalogo["<b>CatalogoEjercicios</b><br/><i>[Servicio de dominio]</i>"]
        irepo["<b>IEjercicioRepository</b><br/><i>[Puerto de salida]</i>"]
    end

    subgraph salida["Adaptadores de salida (driven)"]
        direction LR
        decorator["<b>EjercicioRepositoryLogDecorator</b><br/><i>[Decorator · estructural]</i>"]
        efrepo["<b>EfEjercicioRepository</b><br/><i>[EF Core]</i>"]
    end

    db[("<b>SQLite</b><br/><i>[Base de datos]</i>")]

    atleta --> homeCtrl
    atleta --> calcCtrl
    clienteApi --> apiCtrl

    homeCtrl -->|"Invoca"| iservice
    apiCtrl -->|"Invoca"| iservice
    calcCtrl -->|"Usa"| calc
    homeCtrl -->|"Consulta fichas"| catalogo

    iservice -.->|"implementado por"| service
    service -->|"delega la sugerencia"| selector
    selector -->|"selecciona / ejecuta"| strategies
    service -->|"persiste / consulta"| irepo

    irepo -.->|"DI resuelve a"| decorator
    decorator -->|"envuelve (delega en)"| efrepo
    efrepo -->|"Lee / escribe · EF Core / SQL"| db

    classDef persona fill:#08427b,stroke:#052e56,color:#ffffff;
    classDef componente fill:#85bbf0,stroke:#5d82a8,color:#000000;
    classDef datos fill:#438dd5,stroke:#2e6295,color:#ffffff;
    class atleta,clienteApi persona
    class homeCtrl,calcCtrl,apiCtrl,iservice,service,selector,strategies,calc,catalogo,irepo,decorator,efrepo componente
    class db datos
```

**Notas del nivel**
- **Patrón Strategy (comportamiento):** `SelectorEstrategiaProgresion` + las cuatro
  `IEstrategiaProgresion` encapsulan algoritmos intercambiables de progresión. Agregar uno nuevo no
  cambia al `EjercicioService`.
- **Patrón Decorator (estructural):** `EjercicioRepositoryLogDecorator` envuelve a
  `EfEjercicioRepository`; el núcleo solo ve el puerto `IEjercicioRepository`.
- Los controladores dependen **solo de puertos y servicios de dominio**, nunca de EF Core — así se
  mantiene la regla de dependencias de la arquitectura hexagonal (ver ADR-03 y ADR-04).

---

## Declaración de uso de IA

Para elaborar esta documentación C4 se utilizó una herramienta de inteligencia artificial
(**Claude**, asistente de código) con el siguiente alcance:

- **Qué se usó:** apoyo para **redactar y estructurar** los tres diagramas C4 en sintaxis Mermaid y
  las notas explicativas de cada nivel, a partir de la inspección del código real del repositorio
  (controladores, puertos, servicios y patrones ya implementados).
- **Qué NO hizo la IA:** no diseñó ni modificó la arquitectura del sistema. La arquitectura
  hexagonal, los patrones GoF (Strategy y Decorator) y las decisiones técnicas ya existían en el
  proyecto y están documentadas en los ADR-01 a ADR-04.
- **Verificación:** el autor revisó que cada componente, contenedor y relación de los diagramas
  correspondiera con el código fuente y las decisiones previas del proyecto.
- **Responsabilidad:** el contenido final, su exactitud y la entrega son responsabilidad del autor,
  **Josué Enmanuel Poot Mateo**.
