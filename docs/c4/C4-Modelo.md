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
