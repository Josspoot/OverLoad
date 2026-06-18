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
    Task RegistrarAsync(Ejercicio ejercicio);
    Task ActualizarCargaAsync(int id, int series, int repeticiones, double peso);
    Task EliminarAsync(int id);
}
