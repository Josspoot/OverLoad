namespace OverLoad.Models;

/// <summary>
/// Alimento registrado por un usuario en su Bitácora en un día concreto. Los
/// valores nutricionales ya están escalados a la cantidad consumida (en gramos).
/// </summary>
public class RegistroAlimento
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public double Gramos { get; set; }

    public double Calorias { get; set; }
    public double Proteina { get; set; }
    public double Carbohidrato { get; set; }
    public double Grasa { get; set; }

    public string CodigoBarras { get; set; } = string.Empty;
}
