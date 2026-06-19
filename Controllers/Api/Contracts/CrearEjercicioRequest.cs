using System.ComponentModel.DataAnnotations;
using OverLoad.Models;

namespace OverLoad.Controllers.Api.Contracts;

/// <summary>
/// Datos necesarios para registrar un nuevo ejercicio a través de la API.
/// </summary>
public class CrearEjercicioRequest
{
    /// <summary>Nombre del ejercicio. Ej: "Peso Muerto".</summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Área de enfoque muscular. Ej: "Espalda".</summary>
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Enfoque { get; set; } = string.Empty;

    /// <summary>Número de series (1-100).</summary>
    [Range(1, 100)]
    public int Series { get; set; }

    /// <summary>Repeticiones por serie (1-1000).</summary>
    [Range(1, 1000)]
    public int Repeticiones { get; set; }

    /// <summary>Peso utilizado en kilogramos (0-1000).</summary>
    [Range(0, 1000)]
    public double Peso { get; set; }

    /// <summary>Nivel de esfuerzo percibido (1-10).</summary>
    [Range(1, 10)]
    public int Esfuerzo { get; set; }

    /// <summary>Convierte el contrato de entrada en la entidad de dominio.</summary>
    public Ejercicio ToEntity() => new()
    {
        Nombre = Nombre,
        Enfoque = Enfoque,
        Series = Series,
        Repeticiones = Repeticiones,
        Peso = Peso,
        Esfuerzo = Esfuerzo
    };
}
