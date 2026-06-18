using OverLoad.Application.Ports;
using OverLoad.Models;

namespace OverLoad.Application.Services;

/// <summary>
/// Servicio de aplicación. Implementa los casos de uso del puerto de entrada
/// apoyándose únicamente en el puerto de salida (IEjercicioRepository).
/// No depende de ASP.NET ni de EF Core, por lo que la lógica es probable de
/// forma aislada y reutilizable desde cualquier adaptador de entrada.
/// </summary>
public class EjercicioService(IEjercicioRepository repositorio) : IEjercicioService
{
    public Task<IReadOnlyList<Ejercicio>> ListarAsync() =>
        repositorio.ObtenerTodosAsync();

    public Task RegistrarAsync(Ejercicio ejercicio) =>
        repositorio.AgregarAsync(ejercicio);

    public async Task ActualizarCargaAsync(int id, int series, int repeticiones, double peso)
    {
        var ejercicio = await repositorio.ObtenerPorIdAsync(id);
        if (ejercicio is null) return;

        ejercicio.Series = series;
        ejercicio.Repeticiones = repeticiones;
        ejercicio.Peso = peso;
        await repositorio.ActualizarAsync(ejercicio);
    }

    public Task EliminarAsync(int id) =>
        repositorio.EliminarAsync(id);
}
