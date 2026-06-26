using OverLoad.Models;

namespace OverLoad.Application.Progresion;

/// <summary>
/// Estrategia de progresión de carga (patrón GoF Strategy).
/// Encapsula UNA forma de aplicar el principio de sobrecarga progresiva:
/// dado el estado actual de un ejercicio, calcula la carga sugerida para la
/// siguiente sesión. Cada algoritmo (por peso, por repeticiones, por series,
/// doble progresión) es una implementación intercambiable de este contrato.
/// El núcleo trabaja contra esta interfaz, no contra una fórmula fija.
/// </summary>
public interface IEstrategiaProgresion
{
    /// <summary>Identificador estable usado para seleccionar la estrategia (p. ej. "peso").</summary>
    string Clave { get; }

    /// <summary>Nombre legible de la estrategia.</summary>
    string Nombre { get; }

    /// <summary>Calcula la carga sugerida a partir del estado actual del ejercicio.</summary>
    CargaSugerida Sugerir(Ejercicio actual);
}
