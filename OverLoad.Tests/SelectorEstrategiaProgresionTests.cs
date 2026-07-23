using OverLoad.Application.Progresion;
using OverLoad.Application.Progresion.Estrategias;

namespace OverLoad.Tests;

/// <summary>
/// Pruebas de <see cref="SelectorEstrategiaProgresion"/>: el "contexto" del patrón
/// Strategy que resuelve una estrategia por su clave. Se prueba porque es el punto
/// que desacopla el núcleo de las implementaciones concretas; debe resolver sin
/// distinguir mayúsculas y devolver null ante una clave inexistente.
/// </summary>
public class SelectorEstrategiaProgresionTests
{
    private static SelectorEstrategiaProgresion CrearSelector() =>
        new(new IEstrategiaProgresion[] { new ProgresionPorPeso(), new DobleProgresion() });

    [Fact]
    public void Resolver_ClaveExistente_DevuelveLaEstrategia()
    {
        // Arrange
        var selector = CrearSelector();

        // Act
        var estrategia = selector.Resolver("peso");

        // Assert
        Assert.NotNull(estrategia);
        Assert.Equal("peso", estrategia!.Clave);
    }

    [Fact]
    public void Resolver_IgnoraMayusculas()
    {
        // Arrange
        var selector = CrearSelector();

        // Act
        var estrategia = selector.Resolver("PESO");

        // Assert
        Assert.NotNull(estrategia);
        Assert.Equal("peso", estrategia!.Clave);
    }

    [Fact]
    public void Resolver_ClaveInexistente_DevuelveNull()
    {
        // Arrange
        var selector = CrearSelector();

        // Act
        var estrategia = selector.Resolver("no-existe");

        // Assert
        Assert.Null(estrategia);
    }

    [Fact]
    public void ClavesDisponibles_ListaTodasLasEstrategiasRegistradas()
    {
        // Arrange
        var selector = CrearSelector();

        // Act
        var claves = selector.ClavesDisponibles;

        // Assert
        Assert.Equal(2, claves.Count);
        Assert.Contains("peso", claves);
        Assert.Contains("doble", claves);
    }
}
