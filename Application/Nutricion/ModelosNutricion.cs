namespace OverLoad.Application.Nutricion;

/// <summary>Datos de entrada para el cálculo metabólico.</summary>
public record DatosMetabolicos(
    Sexo Sexo,
    int Edad,
    double PesoKg,
    double AlturaCm,
    NivelActividad NivelActividad,
    ObjetivoNutricional Objetivo,
    FormulaTmb Formula,
    double CambioPesoKg);

/// <summary>Calorías de mantenimiento para un nivel de actividad concreto.</summary>
public record CategoriaMantenimiento(
    string Nivel,
    string Descripcion,
    double Factor,
    double Calorias);

/// <summary>Reparto de macronutrientes (en gramos y kcal) para las calorías objetivo.</summary>
public record DesgloseMacros(
    int ProteinaG, int ProteinaKcal,
    int GrasaG, int GrasaKcal,
    int CarbohidratoG, int CarboKcal);

/// <summary>Estimación del tiempo para alcanzar un cambio de peso objetivo.</summary>
public record EstimacionTiempo(
    double CambioPesoKg,
    double RitmoSemanalKg,
    int Semanas,
    double Meses,
    string Nota);

/// <summary>Resultado completo del cálculo: TMB, TDEE, objetivo, desglose y estimaciones.</summary>
public record ResultadoMetabolico(
    double Tmb,
    double Tdee,
    double CaloriasObjetivo,
    int AjusteDiario,
    string NombreFormula,
    string FormulaGenerica,
    string FormulaCalculada,
    string NombreObjetivo,
    string DescripcionObjetivo,
    string NombreActividad,
    double FactorActividad,
    IReadOnlyList<CategoriaMantenimiento> Mantenimiento,
    DesgloseMacros Macros,
    EstimacionTiempo? Tiempo);
