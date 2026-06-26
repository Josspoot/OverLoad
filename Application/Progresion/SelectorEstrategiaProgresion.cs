namespace OverLoad.Application.Progresion;

/// <summary>
/// Contexto del patrón Strategy: resuelve la estrategia de progresión adecuada a
/// partir de su clave. Recibe por DI todas las estrategias registradas, de modo
/// que agregar una nueva no obliga a modificar este selector (principio Open/Closed).
/// </summary>
public class SelectorEstrategiaProgresion(IEnumerable<IEstrategiaProgresion> estrategias)
{
    /// <summary>Claves de las estrategias disponibles (p. ej. "peso", "doble").</summary>
    public IReadOnlyList<string> ClavesDisponibles =>
        estrategias.Select(e => e.Clave).ToList();

    /// <summary>
    /// Devuelve la estrategia cuya clave coincide (sin distinguir mayúsculas), o
    /// <c>null</c> si no existe.
    /// </summary>
    public IEstrategiaProgresion? Resolver(string clave) =>
        estrategias.FirstOrDefault(e =>
            e.Clave.Equals(clave, StringComparison.OrdinalIgnoreCase));
}
