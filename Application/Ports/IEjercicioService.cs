using OverLoad.Application.Progresion;
using OverLoad.Models;

namespace OverLoad.Application.Ports;

/// <summary>
/// Puerto de entrada (driving). Expone los casos de uso que el exterior
/// (web MVC hoy; una API REST o un cliente móvil mañana) puede solicitar al
/// núcleo. Los adaptadores de entrada dependen de esta interfaz, no al revés.
/// </summary>
public interface IEjercicioService
{
    Task<IReadOnlyList<Ejercicio>> ListarAsync();
    Task<Ejercicio?> ObtenerAsync(int id);
    Task RegistrarAsync(Ejercicio ejercicio);
    Task ActualizarCargaAsync(int id, int series, int repeticiones, double peso);
    Task EliminarAsync(int id);

    /// <summary>Claves de las estrategias de progresión disponibles.</summary>
    IReadOnlyList<string> EstrategiasDeProgresion();

    /// <summary>
    /// Sugiere la carga para la próxima sesión de un ejercicio aplicando la
    /// estrategia de progresión indicada (patrón Strategy).
    /// Devuelve <c>null</c> si el ejercicio no existe o la estrategia no es válida.
    /// </summary>
    Task<CargaSugerida?> SugerirProgresionAsync(int id, string estrategia);
}
