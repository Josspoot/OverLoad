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
