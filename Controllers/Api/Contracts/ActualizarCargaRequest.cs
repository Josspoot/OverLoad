using System.ComponentModel.DataAnnotations;

namespace OverLoad.Controllers.Api.Contracts;

/// <summary>
/// Datos para actualizar la carga (series, repeticiones y peso) de un ejercicio.
/// </summary>
public class ActualizarCargaRequest
{
    /// <summary>Nuevo número de series (1-100).</summary>
    [Range(1, 100)]
    public int Series { get; set; }

    /// <summary>Nuevas repeticiones por serie (1-1000).</summary>
    [Range(1, 1000)]
    public int Repeticiones { get; set; }

    /// <summary>Nuevo peso en kilogramos (0-1000).</summary>
    [Range(0, 1000)]
    public double Peso { get; set; }
}
