using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OverLoad.Application.Nutricion;

namespace OverLoad.Infrastructure.OpenFoodFacts;

/// <summary>
/// Adaptador de salida del puerto <see cref="IBuscadorAlimentos"/>. Consulta la
/// API pública de Open Food Facts (endpoint de búsqueda) y normaliza el resultado
/// a <see cref="AlimentoEncontrado"/> con valores por 100 g. Ante cualquier error
/// de red o formato devuelve una lista vacía (degradación elegante).
/// </summary>
public class OpenFoodFactsClient(HttpClient http) : IBuscadorAlimentos
{
    private static readonly JsonSerializerOptions Opciones = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<AlimentoEncontrado>> BuscarAsync(string termino, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termino)) return [];

        var url = $"cgi/search.pl?search_terms={Uri.EscapeDataString(termino)}" +
                  "&search_simple=1&action=process&json=1&page_size=20";

        OffRespuesta? datos;
        try
        {
            datos = await http.GetFromJsonAsync<OffRespuesta>(url, Opciones, ct);
        }
        catch
        {
            return [];
        }

        if (datos?.Products is null) return [];

        return datos.Products
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
            .Select(p => new AlimentoEncontrado(
                CodigoBarras: p.Code ?? string.Empty,
                Nombre: p.ProductName!.Trim(),
                Marca: (p.Brands ?? string.Empty).Split(',').FirstOrDefault()?.Trim() ?? string.Empty,
                CaloriasPor100g: Redondear(p.Nutriments?.EnergyKcal100g),
                ProteinaPor100g: Redondear(p.Nutriments?.Proteins100g),
                CarboPor100g: Redondear(p.Nutriments?.Carbs100g),
                GrasaPor100g: Redondear(p.Nutriments?.Fat100g),
                ImagenUrl: p.ImageSmallUrl))
            .Where(a => a.CaloriasPor100g > 0)
            .ToList();
    }

    private static double Redondear(double? valor) => valor.HasValue ? Math.Round(valor.Value, 1) : 0;

    // DTOs internos que mapean la respuesta de Open Food Facts.
    private sealed class OffRespuesta
    {
        public List<OffProducto>? Products { get; set; }
    }

    private sealed class OffProducto
    {
        [JsonPropertyName("product_name")] public string? ProductName { get; set; }
        public string? Brands { get; set; }
        public string? Code { get; set; }
        [JsonPropertyName("image_small_url")] public string? ImageSmallUrl { get; set; }
        public OffNutrimentos? Nutriments { get; set; }
    }

    private sealed class OffNutrimentos
    {
        [JsonPropertyName("energy-kcal_100g")] public double? EnergyKcal100g { get; set; }
        [JsonPropertyName("proteins_100g")] public double? Proteins100g { get; set; }
        [JsonPropertyName("carbohydrates_100g")] public double? Carbs100g { get; set; }
        [JsonPropertyName("fat_100g")] public double? Fat100g { get; set; }
    }
}
