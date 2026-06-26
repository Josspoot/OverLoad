using OverLoad.Application.Progresion;

namespace OverLoad.Controllers.Api.Contracts;

/// <summary>
/// Representación pública de una sugerencia de progresión de carga.
/// </summary>
public class SugerenciaProgresionResponse
{
    /// <summary>Nombre de la estrategia de progresión aplicada.</summary>
    public string Estrategia { get; set; } = string.Empty;

    /// <summary>Series sugeridas para la próxima sesión.</summary>
    public int Series { get; set; }

    /// <summary>Repeticiones sugeridas por serie.</summary>
    public int Repeticiones { get; set; }

    /// <summary>Peso sugerido en kilogramos.</summary>
    public double Peso { get; set; }

    /// <summary>Explicación de por qué se sugiere esta carga.</summary>
    public string Justificacion { get; set; } = string.Empty;

    /// <summary>Crea la respuesta a partir del resultado del núcleo.</summary>
    public static SugerenciaProgresionResponse From(CargaSugerida c) => new()
    {
        Estrategia = c.Estrategia,
        Series = c.Series,
        Repeticiones = c.Repeticiones,
        Peso = c.Peso,
        Justificacion = c.Justificacion
    };
}
