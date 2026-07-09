using OverLoad.Application.Nutricion;

namespace OverLoad.Models;

/// <summary>
/// Modelo de la vista de la calculadora: transporta los datos del formulario y,
/// tras el cálculo, el resultado metabólico para mostrarlo.
/// </summary>
public class CalculadoraViewModel
{
    public Sexo Sexo { get; set; } = Sexo.Hombre;
    public int Edad { get; set; } = 25;
    public double PesoKg { get; set; } = 70;
    public double AlturaCm { get; set; } = 170;
    public NivelActividad NivelActividad { get; set; } = NivelActividad.Moderado;
    public ObjetivoNutricional Objetivo { get; set; } = ObjetivoNutricional.Mantenimiento;
    public FormulaTmb Formula { get; set; } = FormulaTmb.MifflinStJeor;

    /// <summary>Kilogramos que el usuario desea ganar o perder (para estimar el tiempo).</summary>
    public double CambioPesoKg { get; set; } = 5;

    /// <summary>Resultado del cálculo; null mientras no se haya enviado el formulario.</summary>
    public ResultadoMetabolico? Resultado { get; set; }

    /// <summary>True si el resultado se guardó como perfil activo (usuario autenticado).</summary>
    public bool PerfilGuardado { get; set; }
}
