using OverLoad.Models;

namespace OverLoad.Application.Progresion.Estrategias;

/// <summary>
/// Doble progresión: se suben repeticiones hasta alcanzar un tope; al llegar al
/// tope, se reinician al piso y se incrementa el peso. Combina volumen e
/// intensidad de forma escalonada, una de las progresiones más usadas en la práctica.
/// </summary>
public class DobleProgresion : IEstrategiaProgresion
{
    private const int TopeReps = 12;
    private const int PisoReps = 8;
    private const double IncrementoKg = 2.5;

    public string Clave => "doble";
    public string Nombre => "Doble progresión";

    public CargaSugerida Sugerir(Ejercicio actual)
    {
        if (actual.Repeticiones < TopeReps)
        {
            return new CargaSugerida(
                Estrategia: Nombre,
                Series: actual.Series,
                Repeticiones: actual.Repeticiones + 1,
                Peso: actual.Peso,
                Justificacion: $"Aún no llegas al tope de {TopeReps} reps: suma 1 repetición con el mismo " +
                               "peso hasta alcanzarlo.");
        }

        return new CargaSugerida(
            Estrategia: Nombre,
            Series: actual.Series,
            Repeticiones: PisoReps,
            Peso: actual.Peso + IncrementoKg,
            Justificacion: $"Alcanzaste el tope de {TopeReps} reps: sube {IncrementoKg} kg y reinicia a " +
                           $"{PisoReps} repeticiones para volver a progresar.");
    }
}
