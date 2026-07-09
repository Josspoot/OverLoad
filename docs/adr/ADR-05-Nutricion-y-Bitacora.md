# ADR-05: Módulo de nutrición, Bitácora y recordatorio de progresión

| Campo  | Valor |
|--------|-------|
| Autor  | Josué Enmanuel Poot Mateo |
| Fecha  | 08/07/2026 |
| Estado | `Aceptado` |

---

## Contexto

Sobre la base ya funcional (arquitectura hexagonal, patrones GoF, librería y calculadora), se
incorporaron funcionalidades centradas en el usuario que requieren **estado por persona** y una
**fuente de datos externa**:

1. **Librería editable**: el usuario puede crear sus propios ejercicios (fichas) y enviarlos al Tracker.
2. **Recordatorio de sobrecarga progresiva**: avisar cuándo conviene progresar cada ejercicio.
3. **Bitácora nutricional**: registrar alimentos por día, medir calorías/macros consumidos y
   compararlos contra el objetivo del usuario, tomando datos de una base pública de alimentos.

Estas necesidades introdujeron tres decisiones arquitectónicas relevantes.

---

## Decisión

### 1. Datos por usuario mediante ASP.NET Identity

Los ejercicios personalizados, el Tracker, el perfil metabólico y el registro de alimentos se
asocian al `UserId` del usuario autenticado. Se introduce el puerto **`IUsuarioActual`** (implementado
por un adaptador que lee el `HttpContext`), de modo que el núcleo obtiene la identidad **sin acoplarse
a ASP.NET**. Las páginas correspondientes exigen sesión iniciada (`[Authorize]`). La API REST se
mantiene anónima y opera sobre el conjunto global (sin dueño), como adaptador máquina-a-máquina.

### 2. El perfil metabólico como puente Calculadora → Bitácora

Al calcular en la Calculadora, si el usuario está autenticado, el resultado (TDEE, calorías objetivo
y macros) se guarda como **`PerfilMetabolico`** (un registro por usuario, se sobrescribe). La Bitácora
lee ese perfil para fijar la **meta diaria** y dibujar la barra de progreso según el objetivo planteado.

### 3. Open Food Facts detrás de un puerto

La búsqueda de alimentos se define como el puerto **`IBuscadorAlimentos`**; su adaptador
**`OpenFoodFactsClient`** consume la API pública de [Open Food Facts](https://world.openfoodfacts.org/)
con un `HttpClient` tipado, normaliza los valores a "por 100 g" y degrada de forma elegante
(devuelve lista vacía) ante errores de red. El núcleo no conoce el proveedor concreto: mañana podría
cambiarse por otra base de alimentos sin tocar la Bitácora.

### 4. Recordatorio de progresión como política de dominio

Se añade `UltimaActualizacion` a cada ejercicio del Tracker (se reinicia con cada ajuste de carga) y
la clase **`PoliticaProgresion`** encapsula la regla: se sugiere progresar tras **14 días** (ventana de
1-2 semanas respaldada por la evidencia). El aviso se muestra en la app (banner + badge) y, con permiso,
como **notificación del navegador** (Web Notifications API).

> **Periodo de 14 días — base científica.** La literatura recomienda aplicar/reevaluar la sobrecarga
> progresiva en una ventana de 1 a 2 semanas por ejercicio (principiantes casi cada sesión; intermedios
> 1-2 semanas), con reevaluación/deload cada 4-6 semanas. Fuentes: *Stronger by Science — Progressive
> Overload Strategies*; *Cleveland Clinic — Progressive Overload*; *NASM — Progressive Overload Explained*.

---

## Alternativas consideradas

| Decisión | Alternativa descartada | Motivo |
|----------|------------------------|--------|
| Datos por usuario (Identity) | Estado global sin login | No permite bitácoras ni librerías personales; se buscaba experiencia individual. |
| `IUsuarioActual` (puerto) | Leer `HttpContext` en el núcleo | Acoplaría el dominio a ASP.NET y rompería la arquitectura hexagonal. |
| Open Food Facts tras puerto | Llamar al `HttpClient` desde el controlador | Acopla la web al proveedor y dificulta cambiarlo o probarlo. |
| Notificación del navegador | Solo texto en la página | Se pidió explícitamente un "recordatorio a modo de notificación". |

---

## Consecuencias

**Positivas**
- Cada usuario tiene su propio Tracker, librería, perfil y bitácora.
- La Calculadora y la Bitácora quedan conectadas por un contrato claro (`PerfilMetabolico`).
- La dependencia de Open Food Facts está aislada tras un puerto y no contamina el dominio.
- La regla de progresión vive en un único lugar (`PoliticaProgresion`), fácil de ajustar.

**Negativas / compromisos**
- Las funciones nuevas requieren iniciar sesión.
- La Bitácora depende de la disponibilidad de un servicio externo (mitigado con degradación elegante).
- La API REST y el Tracker web operan sobre conjuntos de datos distintos (global vs. por usuario).
