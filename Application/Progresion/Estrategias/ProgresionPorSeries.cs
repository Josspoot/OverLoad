using OverLoad.Models;

namespace OverLoad.Application.Progresion.Estrategias;

/// <summary>
/// Progresión orientada a volumen: mantiene peso y repeticiones, y agrega una
/// serie adicional al ejercicio.
/// </summary>
public class ProgresionPorSeries : IEstrategiaProgresion
{
    private const int IncrementoSeries = 1;

    public string Clave => "series";
    public string Nombre => "Progresión por series";

    public CargaSugerida Sugerir(Ejercicio actual) => new(
        Estrategia: Nombre,
        Series: actual.Series + IncrementoSeries,
        Repeticiones: actual.Repeticiones,
        Peso: actual.Peso,
        Justificacion: $"Agregar {IncrementoSeries} serie incrementa el volumen de entrenamiento " +
                       "manteniendo la técnica y la intensidad actuales.");
}
