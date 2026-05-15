namespace OverLoad.Models;

public class Ejercicio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Enfoque { get; set; } = string.Empty;
    public int Series { get; set; }
    public int Repeticiones { get; set; }
    public double Peso { get; set; }
    public int Esfuerzo { get; set; }
}