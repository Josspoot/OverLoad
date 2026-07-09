using System.ComponentModel.DataAnnotations;

namespace OverLoad.Models;

/// <summary>Formulario para dar de alta un ejercicio personalizado en la librería.</summary>
public class NuevoEjercicioViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre del ejercicio")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Elige un grupo muscular.")]
    [Display(Name = "Grupo muscular")]
    public string Grupo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Explica cómo se hace el ejercicio.")]
    [Display(Name = "¿Cómo se hace?")]
    public string ComoHacerlo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describe qué se debe sentir.")]
    [Display(Name = "¿Qué se debe sentir?")]
    public string QueSeDebeSentir { get; set; } = string.Empty;

    [Display(Name = "Recomendaciones")]
    public string Recomendaciones { get; set; } = string.Empty;

    [Display(Name = "Equipo extra (cinturón, straps, etc.)")]
    public string EquipoExtra { get; set; } = string.Empty;

    /// <summary>Grupos musculares sugeridos para el desplegable.</summary>
    public static readonly string[] Grupos =
        ["Pecho", "Espalda", "Piernas", "Hombros", "Brazos", "Core"];
}
