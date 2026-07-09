namespace OverLoad.Application.Nutricion;

/// <summary>
/// Alimento devuelto por una búsqueda en Open Food Facts, con sus valores
/// nutricionales normalizados por cada 100 g.
/// </summary>
public record AlimentoEncontrado(
    string CodigoBarras,
    string Nombre,
    string Marca,
    double CaloriasPor100g,
    double ProteinaPor100g,
    double CarboPor100g,
    double GrasaPor100g,
    string? ImagenUrl);
