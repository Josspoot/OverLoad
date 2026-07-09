using Microsoft.EntityFrameworkCore;
using OverLoad.Application.Ports;
using OverLoad.Data;
using OverLoad.Models;

namespace OverLoad.Infrastructure.Persistence;

/// <summary>
/// Adaptador de salida (driven). Implementa el puerto IEjercicioRepository
/// usando Entity Framework Core sobre SQLite. Filtra por el usuario autenticado
/// (<see cref="IUsuarioActual"/>): cada usuario ve solo su propio Tracker; las
/// peticiones anónimas (API) operan sobre el conjunto global sin dueño.
/// </summary>
public class EfEjercicioRepository(ApplicationDbContext context, IUsuarioActual usuario) : IEjercicioRepository
{
    public async Task<IReadOnlyList<Ejercicio>> ObtenerTodosAsync() =>
        await context.Ejercicios.AsNoTracking()
            .Where(e => e.UserId == usuario.Id)
            .ToListAsync();

    public async Task<Ejercicio?> ObtenerPorIdAsync(int id)
    {
        var ejercicio = await context.Ejercicios.FindAsync(id);
        return ejercicio is null || ejercicio.UserId != usuario.Id ? null : ejercicio;
    }

    public async Task AgregarAsync(Ejercicio ejercicio)
    {
        ejercicio.UserId = usuario.Id;
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
        if (ejercicio is null || ejercicio.UserId != usuario.Id) return;

        context.Ejercicios.Remove(ejercicio);
        await context.SaveChangesAsync();
    }
}
