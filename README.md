# OverLoad

Aplicación web para el seguimiento y control de entrenamientos físicos. Permite registrar ejercicios, monitorear series, repeticiones, peso y nivel de esfuerzo, con el objetivo de apoyar la progresión en el entrenamiento mediante el principio de sobrecarga progresiva.

<img width="1440" height="810" alt="image" src="https://github.com/user-attachments/assets/43a5490f-dd39-43f6-9c45-5fe78e621e60" />
<img width="1440" height="810" alt="image" src="https://github.com/user-attachments/assets/fdd53e9e-1b4f-40fc-881e-188497f9c58b" />
<img width="1440" height="808" alt="image" src="https://github.com/user-attachments/assets/963b537c-13e9-4e62-9220-28988c2ae70a" />

---

## Tecnologías

- **ASP.NET Core MVC** (.NET 10)
- **Entity Framework Core** + SQLite
- **ASP.NET Identity** (autenticación de usuarios)
- **Razor Views** (HTML + CSS con modo oscuro)
- **Bootstrap 5**

---

## Funcionalidades

- Registro de ejercicios con nombre, área de enfoque, series, repeticiones, peso y esfuerzo
- Actualización de carga (series, repeticiones y peso) por ejercicio
- Eliminación de ejercicios del tracker
- Autenticación de usuarios con ASP.NET Identity
- Diseño oscuro optimizado para uso en gimnasio

---

## Arquitectura

El proyecto evoluciona hacia una **Arquitectura Hexagonal (Puertos y Adaptadores)** para separar la lógica de negocio de los detalles de infraestructura. El objetivo es que el mismo núcleo de dominio pueda:

- Exponerse a múltiples canales de entrada (sitio web MVC y, a futuro, un cliente móvil mediante una API REST).
- Persistir los datos de forma intercambiable a través de puertos: adaptador de **archivos (JSON/CSV)** o adaptador **SQLite/EF Core**.

Esta decisión está documentada en el [`ADR-03`](docs/adr/ADR-03-arquitectura-hexagonal.md) (estado: Propuesto).

---

## Estructura del Proyecto

<pre>
OverLoad/
├── Controllers/        # HomeController, TrackerController, LibreriaController
├── Models/             # Ejercicio.cs, ErrorViewModel.cs
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
- [`ADR-03`](docs/adr/ADR-03-arquitectura-hexagonal.md) — Adopción de Arquitectura Hexagonal (puertos y adaptadores) para soportar múltiples canales (web/móvil) y persistencia intercambiable (archivos/SQLite)

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

Cambios que hice:
- Nueva sección "Arquitectura" que explica el giro hacia hexagonal y enlaza al ADR-03.
- Lista de ADRs ampliada con el ADR-03.
- Árbol de estructura actualizado: el comentario de docs/adr/ ahora incluye ADR-03.
