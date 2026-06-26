using OverLoad.Models;

namespace OverLoad.Application.Progresion.Estrategias;

/// <summary>
/// Progresión orientada a fuerza: mantiene series y repeticiones, y aumenta el
/// peso en un incremento fijo. Es la progresión clásica de levantamientos pesados.
/// </summary>
public class ProgresionPorPeso : IEstrategiaProgresion
{
    private const double IncrementoKg = 2.5;

    public string Clave => "peso";
    public string Nombre => "Progresión por peso";

    public CargaSugerida Sugerir(Ejercicio actual) => new(
        Estrategia: Nombre,
        Series: actual.Series,
        Repeticiones: actual.Repeticiones,
        Peso: actual.Peso + IncrementoKg,
        Justificacion: $"Subir {IncrementoKg} kg manteniendo {actual.Series}x{actual.Repeticiones} " +
                       "prioriza la ganancia de fuerza mediante sobrecarga en la intensidad.");
}
