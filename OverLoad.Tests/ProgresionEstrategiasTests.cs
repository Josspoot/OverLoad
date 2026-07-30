using OverLoad.Application.Progresion.Estrategias;
using OverLoad.Models;

namespace OverLoad.Tests;

/// <summary>
/// Pruebas de las estrategias de sobrecarga progresiva (patrón GoF Strategy).
/// Se prueban <see cref="ProgresionPorPeso"/> y <see cref="DobleProgresion"/>
/// porque encapsulan las reglas de negocio que sugieren la carga de la próxima
/// sesión; la doble progresión, además, tiene bifurcación (antes/después del tope)
/// que conviene fijar con pruebas.
/// </summary>
public class ProgresionEstrategiasTests
{
    private static Ejercicio Ejercicio(int series, int reps, double peso) =>
        new() { Nombre = "Press banca", Series = series, Repeticiones = reps, Peso = peso };

    [Fact]
    public void ProgresionPorPeso_MantieneSeriesYReps_YSube2Punto5Kg()
    {
        // Arrange: 4x6 con 100 kg.
        var estrategia = new ProgresionPorPeso();
        var actual = Ejercicio(series: 4, reps: 6, peso: 100);

        // Act
        var sugerida = estrategia.Sugerir(actual);

        // Assert: mismas series y reps, peso +2.5 kg.
        Assert.Equal(4, sugerida.Series);
        Assert.Equal(6, sugerida.Repeticiones);
        Assert.Equal(102.5, sugerida.Peso);
    }

    [Fact]
    public void DobleProgresion_DebajoDelTope_SumaUnaRepeticionSinCambiarPeso()
    {
        // Arrange: 8 reps (< tope de 12).
        var estrategia = new DobleProgresion();
        var actual = Ejercicio(series: 3, reps: 8, peso: 50);

        // Act
        var sugerida = estrategia.Sugerir(actual);

        // Assert: +1 rep, mismo peso.
        Assert.Equal(9, sugerida.Repeticiones);
        Assert.Equal(50, sugerida.Peso);
        Assert.Equal(3, sugerida.Series);
    }

    [Fact]
    public void DobleProgresion_EnElTope_SubePesoYReiniciaReps()
    {
        // Arrange: 12 reps (= tope).
        var estrategia = new DobleProgresion();
        var actual = Ejercicio(series: 3, reps: 12, peso: 50);

        // Act
        var sugerida = estrategia.Sugerir(actual);

        // Assert: reinicia a 8 reps y sube 2.5 kg.
        Assert.Equal(8, sugerida.Repeticiones);
        Assert.Equal(52.5, sugerida.Peso);
    }

    [Fact]
    public void Estrategias_ExponenClaveYNombreEstables()
    {
        // Arrange / Act
        var peso = new ProgresionPorPeso();
        var doble = new DobleProgresion();

        // Assert: las claves son las que usa el selector para resolverlas.
        Assert.Equal("peso", peso.Clave);
        Assert.Equal("doble", doble.Clave);
        Assert.False(string.IsNullOrWhiteSpace(peso.Nombre));
        Assert.False(string.IsNullOrWhiteSpace(doble.Nombre));
    }
}
