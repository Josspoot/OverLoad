using System.Diagnostics;
using OverLoad.Application.Ports;
using OverLoad.Models;

namespace OverLoad.Infrastructure.Persistence;

/// <summary>
/// Decorador (patrón GoF Decorator) del puerto de salida <see cref="IEjercicioRepository"/>.
/// Envuelve a otro repositorio (el componente decorado, normalmente
/// <see cref="EfEjercicioRepository"/>) y le agrega de forma transparente el
/// registro (logging) de cada operación de persistencia y su duración, sin
/// modificar la implementación original ni el núcleo.
///
/// Como implementa la misma interfaz que decora, puede apilarse o quitarse por
/// configuración (DI) sin que nadie más se entere: el servicio sigue dependiendo
/// solo de <see cref="IEjercicioRepository"/>.
/// </summary>
public class EjercicioRepositoryLogDecorator(
    IEjercicioRepository inner,
    ILogger<EjercicioRepositoryLogDecorator> logger) : IEjercicioRepository
{
    public Task<IReadOnlyList<Ejercicio>> ObtenerTodosAsync() =>
        MedirAsync(nameof(ObtenerTodosAsync), inner.ObtenerTodosAsync);

    public Task<Ejercicio?> ObtenerPorIdAsync(int id) =>
        MedirAsync($"{nameof(ObtenerPorIdAsync)}(id={id})", () => inner.ObtenerPorIdAsync(id));

    public Task AgregarAsync(Ejercicio ejercicio) =>
        MedirAsync($"{nameof(AgregarAsync)}('{ejercicio.Nombre}')", () => inner.AgregarAsync(ejercicio));

    public Task ActualizarAsync(Ejercicio ejercicio) =>
        MedirAsync($"{nameof(ActualizarAsync)}(id={ejercicio.Id})", () => inner.ActualizarAsync(ejercicio));

    public Task EliminarAsync(int id) =>
        MedirAsync($"{nameof(EliminarAsync)}(id={id})", () => inner.EliminarAsync(id));

    // Envuelve la operación decorada con logging de inicio, fin y duración,
    // registrando también cualquier excepción sin tragársela.
    private async Task<T> MedirAsync<T>(string operacion, Func<Task<T>> accion)
    {
        logger.LogInformation("Repositorio: iniciando {Operacion}", operacion);
        var cronometro = Stopwatch.StartNew();
        try
        {
            var resultado = await accion();
            cronometro.Stop();
            logger.LogInformation("Repositorio: {Operacion} completada en {Ms} ms",
                operacion, cronometro.ElapsedMilliseconds);
            return resultado;
        }
        catch (Exception ex)
        {
            cronometro.Stop();
            logger.LogError(ex, "Repositorio: {Operacion} falló tras {Ms} ms",
                operacion, cronometro.ElapsedMilliseconds);
            throw;
        }
    }

    // Sobrecarga para operaciones sin valor de retorno.
    private async Task MedirAsync(string operacion, Func<Task> accion) =>
        await MedirAsync<object?>(operacion, async () =>
        {
            await accion();
            return null;
        });
}
