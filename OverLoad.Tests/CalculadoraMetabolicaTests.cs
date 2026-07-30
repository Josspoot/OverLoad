using OverLoad.Application.Nutricion;

namespace OverLoad.Tests;

/// <summary>
/// Pruebas de la <see cref="CalculadoraMetabolica"/>: servicio de dominio de lógica
/// pura (TMB, TDEE, ajuste por objetivo y macronutrientes). Se prueba porque
/// concentra las fórmulas del negocio y un error aquí daría al usuario objetivos
/// calóricos incorrectos, sin depender de base de datos ni de ASP.NET.
/// </summary>
public class CalculadoraMetabolicaTests
{
    private static DatosMetabolicos Datos(
        Sexo sexo = Sexo.Hombre,
        int edad = 25,
        double pesoKg = 80,
        double alturaCm = 180,
        NivelActividad nivel = NivelActividad.Sedentario,
        ObjetivoNutricional objetivo = ObjetivoNutricional.Mantenimiento,
        FormulaTmb formula = FormulaTmb.MifflinStJeor,
        double cambioPesoKg = 0)
        => new(sexo, edad, pesoKg, alturaCm, nivel, objetivo, formula, cambioPesoKg);

    [Fact]
    public void Calcular_MifflinHombre_DevuelveTmbYTdeeEsperados()
    {
        // Arrange: hombre 80 kg, 180 cm, 25 años, sedentario, mantenimiento.
        var calc = new CalculadoraMetabolica();
        var datos = Datos();

        // Act
        var r = calc.Calcular(datos);

        // Assert: TMB = 10·80 + 6.25·180 − 5·25 + 5 = 1805; TDEE = 1805 · 1.20 = 2166.
        Assert.Equal("Mifflin-St Jeor", r.NombreFormula);
        Assert.Equal(1805, r.Tmb);
        Assert.Equal(2166, r.Tdee);
        Assert.Equal(0, r.AjusteDiario);
        Assert.Equal(2166, r.CaloriasObjetivo);
    }

    [Fact]
    public void Calcular_MifflinMujer_UsaLaFormulaFemenina()
    {
        // Arrange: mujer 60 kg, 165 cm, 30 años.
        var calc = new CalculadoraMetabolica();
        var datos = Datos(sexo: Sexo.Mujer, edad: 30, pesoKg: 60, alturaCm: 165);

        // Act
        var r = calc.Calcular(datos);

        // Assert: TMB = 10·60 + 6.25·165 − 5·30 − 161 = 1320.25 → 1320.
        Assert.Equal(1320, r.Tmb);
    }

    [Fact]
    public void Calcular_HarrisBenedict_IdentificaLaFormula()
    {
        // Arrange
        var calc = new CalculadoraMetabolica();
        var datos = Datos(formula: FormulaTmb.HarrisBenedict);

        // Act
        var r = calc.Calcular(datos);

        // Assert: 88.362 + 13.397·80 + 4.799·180 − 5.677·25 = 1882.017 → 1882.
        Assert.Equal("Harris-Benedict (revisada)", r.NombreFormula);
        Assert.Equal(1882, r.Tmb);
    }

    [Theory]
    [InlineData(ObjetivoNutricional.Mantenimiento, 0)]
    [InlineData(ObjetivoNutricional.PerderPeso, -500)]
    [InlineData(ObjetivoNutricional.GanarMusculo, 300)]
    [InlineData(ObjetivoNutricional.SubirPeso, 500)]
    public void Calcular_AjustePorObjetivo_AplicaElDeltaCorrecto(ObjetivoNutricional objetivo, int ajusteEsperado)
    {
        // Arrange
        var calc = new CalculadoraMetabolica();
        var datos = Datos(objetivo: objetivo);

        // Act
        var r = calc.Calcular(datos);

        // Assert: las calorías objetivo son el TDEE más el ajuste del objetivo.
        Assert.Equal(ajusteEsperado, r.AjusteDiario);
        Assert.Equal(r.Tdee + ajusteEsperado, r.CaloriasObjetivo);
    }

    [Fact]
    public void Calcular_Macros_ProteinaEsDosGramosPorKilo()
    {
        // Arrange: 80 kg → proteína = 2.0 · 80 = 160 g (640 kcal).
        var calc = new CalculadoraMetabolica();
        var datos = Datos(pesoKg: 80);

        // Act
        var r = calc.Calcular(datos);

        // Assert
        Assert.Equal(160, r.Macros.ProteinaG);
        Assert.Equal(640, r.Macros.ProteinaKcal);
        Assert.True(r.Macros.GrasaG > 0);
        Assert.True(r.Macros.CarbohidratoG > 0);
    }

    [Fact]
    public void Calcular_SinCambioDeObjetivo_NoEstimaTiempo()
    {
        // Arrange: mantenimiento (ajuste 0) no produce estimación de tiempo.
        var calc = new CalculadoraMetabolica();
        var datos = Datos(objetivo: ObjetivoNutricional.Mantenimiento, cambioPesoKg: 5);

        // Act
        var r = calc.Calcular(datos);

        // Assert
        Assert.Null(r.Tiempo);
    }

    [Fact]
    public void Calcular_PerderPesoConCambio_EstimaElTiempo()
    {
        // Arrange: déficit de 500 kcal para perder 5 kg.
        var calc = new CalculadoraMetabolica();
        var datos = Datos(objetivo: ObjetivoNutricional.PerderPeso, cambioPesoKg: 5);

        // Act
        var r = calc.Calcular(datos);

        // Assert
        Assert.NotNull(r.Tiempo);
        Assert.Equal(5, r.Tiempo!.CambioPesoKg);
        Assert.True(r.Tiempo.Semanas > 0);
    }
}
