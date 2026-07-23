using OverLoad.Application.Progresion;

namespace OverLoad.Tests;

/// <summary>
/// Pruebas de <see cref="PoliticaProgresion"/>: política de dominio (clase estática)
/// que decide cuándo toca progresar un ejercicio a partir de fechas. Se prueba
/// porque su aritmética de fechas alimenta el recordatorio del Tracker y es fácil
/// equivocarse en los límites (justo en el día, un día antes, un día después).
/// </summary>
public class PoliticaProgresionTests
{
    [Fact]
    public void DiasPeriodo_EsDeDosSemanas()
    {
        // Arrange / Act / Assert
        Assert.Equal(14, PoliticaProgresion.DiasPeriodo);
    }

    [Fact]
    public void ProximaRevision_SumaCatorceDias()
    {
        // Arrange
        var ultima = new DateTime(2026, 1, 1);

        // Act
        var proxima = PoliticaProgresion.ProximaRevision(ultima);

        // Assert
        Assert.Equal(new DateTime(2026, 1, 15), proxima);
    }

    [Fact]
    public void TocaProgresar_PasadoElPeriodo_EsVerdadero()
    {
        // Arrange: 19 días después de la última actualización (> 14).
        var ultima = new DateTime(2026, 1, 1);
        var ahora = new DateTime(2026, 1, 20);

        // Act
        var toca = PoliticaProgresion.TocaProgresar(ultima, ahora);

        // Assert
        Assert.True(toca);
    }

    [Fact]
    public void TocaProgresar_DentroDelPeriodo_EsFalso()
    {
        // Arrange: solo 9 días después (< 14).
        var ultima = new DateTime(2026, 1, 1);
        var ahora = new DateTime(2026, 1, 10);

        // Act
        var toca = PoliticaProgresion.TocaProgresar(ultima, ahora);

        // Assert
        Assert.False(toca);
    }

    [Fact]
    public void DiasRestantes_CuentaLosDiasHastaLaProximaRevision()
    {
        // Arrange: última 01-01, hoy 01-05 → próxima 01-15 → faltan 10 días.
        var ultima = new DateTime(2026, 1, 1);
        var ahora = new DateTime(2026, 1, 5);

        // Act
        var restantes = PoliticaProgresion.DiasRestantes(ultima, ahora);

        // Assert
        Assert.Equal(10, restantes);
    }

    [Fact]
    public void DiasRestantes_CuandoYaToca_EsCeroOMenor()
    {
        // Arrange: ya pasó el periodo.
        var ultima = new DateTime(2026, 1, 1);
        var ahora = new DateTime(2026, 1, 20);

        // Act
        var restantes = PoliticaProgresion.DiasRestantes(ultima, ahora);

        // Assert
        Assert.True(restantes <= 0);
    }
}
