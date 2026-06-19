using OverLoad.Models;

namespace OverLoad.Controllers.Api.Contracts;

/// <summary>
/// Representación pública de un ejercicio que devuelve la API.
/// </summary>
public class EjercicioResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Enfoque { get; set; } = string.Empty;
    public int Series { get; set; }
    public int Repeticiones { get; set; }
    public double Peso { get; set; }
    public int Esfuerzo { get; set; }

    /// <summary>Crea la respuesta a partir de la entidad de dominio.</summary>
    public static EjercicioResponse From(Ejercicio e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Enfoque = e.Enfoque,
        Series = e.Series,
        Repeticiones = e.Repeticiones,
        Peso = e.Peso,
        Esfuerzo = e.Esfuerzo
    };
}
