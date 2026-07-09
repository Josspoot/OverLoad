namespace OverLoad.Application.Nutricion;

/// <summary>
/// Puerto de salida para buscar alimentos en una fuente externa (Open Food Facts).
/// El núcleo depende de esta abstracción, no del cliente HTTP concreto.
/// </summary>
public interface IBuscadorAlimentos
{
    Task<IReadOnlyList<AlimentoEncontrado>> BuscarAsync(string termino, CancellationToken ct = default);
}
