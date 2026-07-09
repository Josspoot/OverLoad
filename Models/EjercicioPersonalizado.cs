namespace OverLoad.Models;

/// <summary>
/// Ejercicio de la librería creado manualmente por un usuario. Comparte los
/// mismos campos de ficha que el catálogo estático, pero pertenece a un usuario.
/// </summary>
public class EjercicioPersonalizado
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Grupo { get; set; } = string.Empty;
    public string ComoHacerlo { get; set; } = string.Empty;
    public string QueSeDebeSentir { get; set; } = string.Empty;
    public string Recomendaciones { get; set; } = string.Empty;
    public string EquipoExtra { get; set; } = string.Empty;
}
