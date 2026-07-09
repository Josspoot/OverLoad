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
C4Context
    title Nivel 1 - Contexto del sistema OverLoad

    Person(atleta, "Atleta / Usuario", "Persona que registra sus entrenamientos de fuerza, consulta la libreria de ejercicios y usa la calculadora metabolica.")
    Person(cliente_api, "Consumidor de la API", "App movil o cliente externo que consume la API REST para el seguimiento de entrenamientos.")

    System(overload, "OverLoad", "Aplicacion web para registrar y dar seguimiento a entrenamientos de fuerza: ejercicios, series, repeticiones, peso, progresion de cargas, libreria de ejercicios y calculadora metabolica (TMB/TDEE).")

    System_Ext(identity, "ASP.NET Identity", "Gestion de cuentas, registro e inicio de sesion de los usuarios.")

    Rel(atleta, overload, "Registra entrenamientos y consulta la libreria/calculadora", "HTTPS / navegador")
    Rel(cliente_api, overload, "Consume casos de uso de ejercicios y sugerencias de progresion", "HTTPS / JSON (REST)")
    Rel(overload, identity, "Autentica y autoriza usuarios", "ASP.NET Core Identity")

    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
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
C4Container
    title Nivel 2 - Contenedores de OverLoad

    Person(atleta, "Atleta / Usuario", "Usa el navegador web.")
    Person(cliente_api, "Consumidor de la API", "App movil o cliente externo.")

    System_Boundary(overload, "OverLoad") {
        Container(web, "Aplicacion Web MVC", "ASP.NET Core MVC + Razor", "Sirve las paginas: inicio, tracker de ejercicios, libreria y calculadora. Adaptador de entrada (driving).")
        Container(api, "API REST", "ASP.NET Core Web API + Swagger", "Expone los casos de uso de ejercicios y las sugerencias de progresion en JSON. Adaptador de entrada (driving).")
        Container(nucleo, "Nucleo de Aplicacion", "C# / .NET (logica de dominio)", "Casos de uso, puertos (IEjercicioService / IEjercicioRepository), patron Strategy (progresion) y servicios de dominio (calculadora, catalogo). No conoce web ni base de datos.")
        Container(persistencia, "Adaptador de Persistencia", "EF Core + Decorator de logging", "Implementa el puerto de salida IEjercicioRepository sobre EF Core. Envuelto por un decorador de logging (patron Decorator).")
        ContainerDb(db, "Base de datos", "SQLite", "Almacena ejercicios, usuarios de Identity y migraciones.")
    }

    System_Ext(identity, "ASP.NET Identity", "Autenticacion y gestion de cuentas.")

    Rel(atleta, web, "Usa", "HTTPS")
    Rel(cliente_api, api, "Consume", "HTTPS / JSON")
    Rel(web, nucleo, "Invoca casos de uso", "IEjercicioService")
    Rel(api, nucleo, "Invoca casos de uso", "IEjercicioService")
    Rel(nucleo, persistencia, "Persiste y consulta datos", "IEjercicioRepository")
    Rel(persistencia, db, "Lee / escribe", "EF Core / SQL")
    Rel(web, identity, "Autentica usuarios", "ASP.NET Identity")

    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
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
C4Component
    title Nivel 3 - Componentes de OverLoad (nucleo y adaptadores)

    Person(atleta, "Atleta / Usuario", "Navegador web.")
    Person(cliente_api, "Consumidor de la API", "Cliente REST.")

    Container_Boundary(entrada, "Adaptadores de entrada") {
        Component(homeCtrl, "HomeController", "MVC Controller", "Tracker de ejercicios (CRUD) y libreria de ejercicios (accion Privacy).")
        Component(calcCtrl, "CalculadoraController", "MVC Controller", "Calculadora metabolica: recibe datos y muestra TMB/TDEE, macros y estimaciones.")
        Component(apiCtrl, "EjerciciosApiController", "Web API Controller", "Endpoints REST /api/v1/ejercicios: listar, obtener, crear, actualizar carga, sugerir progresion, eliminar.")
    }

    Container_Boundary(nucleo, "Nucleo de Aplicacion (hexagono)") {
        Component(iservice, "IEjercicioService", "Puerto de entrada", "Contrato de casos de uso que exponen los adaptadores.")
        Component(service, "EjercicioService", "Servicio de aplicacion", "Implementa los casos de uso: listar, registrar, actualizar carga, eliminar y sugerir progresion.")

        Component(selector, "SelectorEstrategiaProgresion", "Strategy (contexto)", "Recibe todas las estrategias por DI y resuelve la adecuada por su clave.")
        Component(strategies, "IEstrategiaProgresion + 4 estrategias", "Strategy (comportamiento)", "ProgresionPorPeso, PorRepeticiones, PorSeries y DobleProgresion: algoritmos intercambiables de sobrecarga progresiva.")

        Component(calc, "CalculadoraMetabolica", "Servicio de dominio", "Logica pura de TMB/TDEE (Mifflin-St Jeor / Harris-Benedict), macros y estimacion de tiempo.")
        Component(catalogo, "CatalogoEjercicios", "Servicio de dominio", "Catalogo estatico de 57 fichas de ejercicios de la libreria.")

        Component(irepo, "IEjercicioRepository", "Puerto de salida", "Contrato de persistencia que necesita el nucleo.")
    }

    Container_Boundary(salida, "Adaptadores de salida") {
        Component(decorator, "EjercicioRepositoryLogDecorator", "Decorator (estructural)", "Envuelve al repositorio real y agrega logging/medicion sin tocar el nucleo.")
        Component(efrepo, "EfEjercicioRepository", "Adaptador EF Core", "Implementacion real de IEjercicioRepository sobre EF Core.")
    }

    ContainerDb(db, "SQLite", "Base de datos", "Ejercicios, Identity y migraciones.")

    Rel(atleta, homeCtrl, "Usa", "HTTPS")
    Rel(atleta, calcCtrl, "Usa", "HTTPS")
    Rel(cliente_api, apiCtrl, "Consume", "JSON")

    Rel(homeCtrl, iservice, "Invoca")
    Rel(apiCtrl, iservice, "Invoca")
    Rel(calcCtrl, calc, "Usa")
    Rel(homeCtrl, catalogo, "Consulta fichas")

    Rel(iservice, service, "Implementado por")
    Rel(service, selector, "Delega la sugerencia de carga")
    Rel(selector, strategies, "Selecciona y ejecuta")
    Rel(service, irepo, "Persiste / consulta")

    Rel(irepo, decorator, "Resuelto por DI a")
    Rel(decorator, efrepo, "Envuelve (delega en)")
    Rel(efrepo, db, "Lee / escribe", "EF Core / SQL")

    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```

**Notas del nivel**
- **Patrón Strategy (comportamiento):** `SelectorEstrategiaProgresion` + las cuatro
  `IEstrategiaProgresion` encapsulan algoritmos intercambiables de progresión. Agregar uno nuevo no
  cambia al `EjercicioService`.
- **Patrón Decorator (estructural):** `EjercicioRepositoryLogDecorator` envuelve a
  `EfEjercicioRepository`; el núcleo solo ve el puerto `IEjercicioRepository`.
- Los controladores dependen **solo de puertos y servicios de dominio**, nunca de EF Core — así se
  mantiene la regla de dependencias de la arquitectura hexagonal (ver ADR-03 y ADR-04).
