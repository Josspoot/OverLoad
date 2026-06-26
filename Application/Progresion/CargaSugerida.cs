namespace OverLoad.Application.Progresion;

/// <summary>
/// Resultado de aplicar una estrategia de progresión sobre un ejercicio.
/// Describe la carga sugerida para la próxima sesión y la justifica.
/// Es un objeto del núcleo: no conoce HTTP ni la base de datos.
/// </summary>
/// <param name="Estrategia">Nombre legible de la estrategia aplicada.</param>
/// <param name="Series">Series sugeridas para la próxima sesión.</param>
/// <param name="Repeticiones">Repeticiones sugeridas por serie.</param>
/// <param name="Peso">Peso sugerido en kilogramos.</param>
/// <param name="Justificacion">Explicación de por qué se sugiere esta carga.</param>
public record CargaSugerida(
    string Estrategia,
    int Series,
    int Repeticiones,
    double Peso,
    string Justificacion);
