using OverLoad.Models;

namespace OverLoad.Application.Ports;

/// <summary>
/// Puerto de salida (driven). Define lo que el núcleo necesita para persistir
/// ejercicios, sin conocer la tecnología concreta (SQLite, archivos, etc.).
/// Cualquier adaptador de persistencia debe implementar este contrato.
/// </summary>
public interface IEjercicioRepository
{
    Task<IReadOnlyList<Ejercicio>> ObtenerTodosAsync();
    Task<Ejercicio?> ObtenerPorIdAsync(int id);
    Task AgregarAsync(Ejercicio ejercicio);
    Task ActualizarAsync(Ejercicio ejercicio);
    Task EliminarAsync(int id);
}
