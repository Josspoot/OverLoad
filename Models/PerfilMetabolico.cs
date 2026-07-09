using OverLoad.Application.Nutricion;

namespace OverLoad.Models;

/// <summary>
/// Perfil metabólico activo de un usuario: la última entrada y resultado de la
/// Calculadora. Es el puente entre la Calculadora y la Bitácora, que lo usa para
/// conocer el objetivo y las calorías meta del día.
/// </summary>
public class PerfilMetabolico
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Actualizado { get; set; }

    // Entradas de la calculadora
    public Sexo Sexo { get; set; }
    public int Edad { get; set; }
    public double PesoKg { get; set; }
    public double AlturaCm { get; set; }
    public NivelActividad NivelActividad { get; set; }
    public ObjetivoNutricional Objetivo { get; set; }
    public FormulaTmb Formula { get; set; }

    // Resultados calculados
    public double Tmb { get; set; }
    public double Tdee { get; set; }
    public double CaloriasObjetivo { get; set; }
    public int ProteinaG { get; set; }
    public int CarbohidratoG { get; set; }
    public int GrasaG { get; set; }
    public string NombreObjetivo { get; set; } = string.Empty;
}
