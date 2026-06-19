using Microsoft.EntityFrameworkCore;
using OverLoad.Application.Ports;
using OverLoad.Data;
using OverLoad.Models;

namespace OverLoad.Infrastructure.Persistence;

/// <summary>
/// Adaptador de salida (driven). Implementa el puerto IEjercicioRepository
/// usando Entity Framework Core sobre SQLite. Es intercambiable por otro
/// adaptador (archivos JSON, otra base de datos) sin modificar el núcleo.
/// </summary>
public class EfEjercicioRepository(ApplicationDbContext context) : IEjercicioRepository
{
    public async Task<IReadOnlyList<Ejercicio>> ObtenerTodosAsync() =>
        await context.Ejercicios.AsNoTracking().ToListAsync();

    public async Task<Ejercicio?> ObtenerPorIdAsync(int id) =>
        await context.Ejercicios.FindAsync(id);

    public async Task AgregarAsync(Ejercicio ejercicio)
    {
        context.Ejercicios.Add(ejercicio);
        await context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Ejercicio ejercicio)
    {
        context.Ejercicios.Update(ejercicio);
        await context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var ejercicio = await context.Ejercicios.FindAsync(id);
        if (ejercicio is null) return;

        context.Ejercicios.Remove(ejercicio);
        await context.SaveChangesAsync();
    }
}
