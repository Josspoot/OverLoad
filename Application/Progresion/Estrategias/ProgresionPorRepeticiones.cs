using OverLoad.Models;

namespace OverLoad.Application.Progresion.Estrategias;

/// <summary>
/// Progresión orientada a hipertrofia/resistencia: mantiene el peso y las series,
/// y aumenta las repeticiones por serie en un incremento fijo.
/// </summary>
public class ProgresionPorRepeticiones : IEstrategiaProgresion
{
    private const int IncrementoReps = 1;

    public string Clave => "repeticiones";
    public string Nombre => "Progresión por repeticiones";

    public CargaSugerida Sugerir(Ejercicio actual) => new(
        Estrategia: Nombre,
        Series: actual.Series,
        Repeticiones: actual.Repeticiones + IncrementoReps,
        Peso: actual.Peso,
        Justificacion: $"Sumar {IncrementoReps} repetición con el mismo peso aumenta el volumen total " +
                       "y favorece la hipertrofia sin elevar todavía la intensidad.");
}
