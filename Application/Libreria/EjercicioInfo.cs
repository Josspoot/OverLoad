namespace OverLoad.Application.Libreria;

/// <summary>
/// Ficha descriptiva de un ejercicio de la librería: cómo ejecutarlo, qué se
/// debe sentir, recomendaciones y el equipo extra que conviene usar.
/// El <see cref="Slug"/> identifica al ejercicio y nombra su imagen
/// (wwwroot/img/ejercicios/{slug}.jpg).
/// </summary>
public record EjercicioInfo(
    string Slug,
    string Nombre,
    string Grupo,
    string ComoHacerlo,
    string QueSeDebeSentir,
    string Recomendaciones,
    string EquipoExtra);

/// <summary>Grupo muscular con su lista de ejercicios.</summary>
public record GrupoEjercicios(
    string Nombre,
    IReadOnlyList<EjercicioInfo> Ejercicios);
