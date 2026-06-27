# OverLoad

Aplicación web para el seguimiento y control de entrenamientos físicos. Permite registrar ejercicios, monitorear series, repeticiones, peso y nivel de esfuerzo, con el objetivo de apoyar la progresión en el entrenamiento mediante el principio de sobrecarga progresiva.

<img width="1440" height="810" alt="image" src="https://github.com/user-attachments/assets/43a5490f-dd39-43f6-9c45-5fe78e621e60" />
<img width="1440" height="810" alt="image" src="https://github.com/user-attachments/assets/fdd53e9e-1b4f-40fc-881e-188497f9c58b" />
<img width="1440" height="808" alt="image" src="https://github.com/user-attachments/assets/963b537c-13e9-4e62-9220-28988c2ae70a" />

---

## Tecnologías

- **ASP.NET Core MVC** (.NET 10)
- **Arquitectura Hexagonal** (puertos y adaptadores)
- **Patrones de diseño GoF**: Strategy (comportamiento) y Decorator (estructural)
- **API REST** documentada con **Swagger / OpenAPI** (Swashbuckle)
- **Entity Framework Core** + SQLite
- **ASP.NET Identity** (autenticación de usuarios)
- **Razor Views** (HTML + CSS con modo oscuro)
- **Bootstrap 5**

---

## Funcionalidades

- Registro de ejercicios con nombre, área de enfoque, series, repeticiones, peso y esfuerzo
- Actualización de carga (series, repeticiones y peso) por ejercicio
- Eliminación de ejercicios del tracker
- Sugerencia de sobrecarga progresiva por ejercicio, con estrategias intercambiables (por peso, repeticiones, series o doble progresión)
- Autenticación de usuarios con ASP.NET Identity
- Diseño oscuro optimizado para uso en gimnasio
- API REST (CRUD de ejercicios) con documentación interactiva Swagger UI, lista para futuros clientes (móvil)

---

## Arquitectura

El proyecto evoluciona hacia una **Arquitectura Hexagonal (Puertos y Adaptadores)** para separar la lógica de negocio de los detalles de infraestructura. El objetivo es que el mismo núcleo de dominio pueda:

- Exponerse a múltiples canales de entrada (sitio web MVC y, a futuro, un cliente móvil mediante una API REST).
- Persistir los datos de forma intercambiable a través de puertos: adaptador de **archivos (JSON/CSV)** o adaptador **SQLite/EF Core**.

Esta decisión está documentada en el [`ADR-03`](docs/adr/ADR-03-Arquitectura-hexagonal.md).

---

## API REST

Además del sitio web, el núcleo se expone a través de una **API REST** (adaptador de entrada alterno que consume el mismo `IEjercicioService`). Está documentada con **Swagger / OpenAPI**.

| Verbo | Ruta | Acción |
|-------|------|--------|
| `GET` | `/api/v1/ejercicios` | Listar todos los ejercicios |
| `GET` | `/api/v1/ejercicios/{id}` | Obtener un ejercicio |
| `POST` | `/api/v1/ejercicios` | Crear un ejercicio |
| `PUT` | `/api/v1/ejercicios/{id}/carga` | Actualizar series, repeticiones y peso |
| `GET` | `/api/v1/ejercicios/{id}/sugerencia?estrategia={clave}` | Sugerir la carga de la próxima sesión (estrategias: `peso`, `repeticiones`, `series`, `doble`) |
| `DELETE` | `/api/v1/ejercicios/{id}` | Eliminar un ejercicio |

**Documentación interactiva (Swagger UI):** con la app corriendo en desarrollo, abre `/swagger` en el navegador para explorar y probar los endpoints. La especificación OpenAPI está en `/swagger/v1/swagger.json`.

---

## Estructura del Proyecto

<pre>
OverLoad/
├── Application/        # Nucleo: puertos (IEjercicioService, IEjercicioRepository) y servicio
│   ├── Ports/
│   ├── Services/
│   └── Progresion/     # Patron Strategy: IEstrategiaProgresion + estrategias (ADR-04)
├── Infrastructure/     # Adaptadores de salida
│   └── Persistence/    # EfEjercicioRepository (SQLite/EF Core) + decorador de logging (ADR-04)
├── Controllers/        # Adaptadores de entrada
│   ├── HomeController.cs   # Web MVC
│   ├── LibreriaController.cs
│   └── Api/            # API REST (EjerciciosApiController + Contracts/DTOs)
├── Models/             # Entidad de dominio Ejercicio.cs, ErrorViewModel.cs
├── Views/              # Vistas Razor (Home, Tracker, Shared)
├── Data/               # ApplicationDbContext, Migrations
├── wwwroot/            # Archivos estáticos (CSS, JS)
└── docs/
    └── adr/            # Decisiones de arquitectura (ADR-01, ADR-02, ADR-03)
</pre>

---

## Decisiones de Arquitectura

La documentación formal de las decisiones de diseño se encuentra en [`docs/adr/`](docs/adr/):

- [`ADR-01`](docs/adr/ADR-01-Overload.md) — Elección del stack tecnológico y patrón MVC
- [`ADR-02`](docs/adr/ADR-02-vistas-arquitectonicas.md) — Vistas arquitectónicas del sistema (lógica, física, despliegue y procesos)
- [`ADR-03`](docs/adr/ADR-03-Arquitectura-hexagonal.md) — Adopción de Arquitectura Hexagonal (puertos y adaptadores) para soportar múltiples canales (web/móvil/API REST) y persistencia intercambiable (archivos/SQLite)
- [`ADR-04`](docs/adr/ADR-04-Patrones-GoF.md) — Patrones de diseño GoF: **Strategy** (comportamiento) para las estrategias de progresión de carga y **Decorator** (estructural) para el logging de la persistencia

---

## Autor

**Josué Enmanuel Poot Mateo**
Instituto Tecnológico de Software

---

## Uso de Inteligencia Artificial

En el desarrollo de este proyecto se utilizó inteligencia artificial (Claude, de Anthropic) como herramienta de apoyo en las siguientes áreas:

- **Diseño:** Apoyo en decisiones de estructura y organización del sistema.
- **Corrección de errores:** Identificación y resolución de bugs durante el desarrollo.
- **Documentación:** Asistencia en la redacción de ADRs y diagramas arquitectónicos
