namespace OverLoad.Application.Nutricion;

/// <summary>Sexo biológico, necesario porque las fórmulas de TMB difieren.</summary>
public enum Sexo
{
    Hombre,
    Mujer
}

/// <summary>
/// Nivel de actividad física. Determina el factor por el que se multiplica la
/// TMB para obtener el gasto energético total diario (TDEE).
/// </summary>
public enum NivelActividad
{
    Sedentario,
    Ligero,
    Moderado,
    Activo,
    MuyActivo
}

/// <summary>Objetivo del usuario; ajusta las calorías sobre el mantenimiento (TDEE).</summary>
public enum ObjetivoNutricional
{
    Mantenimiento,
    PerderPeso,
    GanarMusculo,
    SubirPeso
}

/// <summary>Fórmula con la que se estima la Tasa Metabólica Basal (TMB).</summary>
public enum FormulaTmb
{
    MifflinStJeor,
    HarrisBenedict
}
