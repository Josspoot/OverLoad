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

    /// <summary>Dueño del ejercicio (usuario del Tracker). Null = conjunto global (API).</summary>
    public string? UserId { get; set; }

    /// <summary>Fecha (UTC) del último registro/ajuste de carga. Base del recordatorio de progresión.</summary>
    public DateTime UltimaActualizacion { get; set; }
}