using Microsoft.EntityFrameworkCore;
using OverLoad.Application.Ports;
using OverLoad.Data;
using OverLoad.Models;

namespace OverLoad.Infrastructure.Persistence;

/// <summary>
/// Adaptador de salida (EF Core) del puerto <see cref="IEjercicioPersonalizadoRepository"/>.
/// </summary>
public class EfEjercicioPersonalizadoRepository(ApplicationDbContext context) : IEjercicioPersonalizadoRepository
{
    public async Task<IReadOnlyList<EjercicioPersonalizado>> ObtenerDeUsuarioAsync(string userId) =>
        await context.EjerciciosPersonalizados.AsNoTracking()
            .Where(e => e.UserId == userId)
            .ToListAsync();

    public async Task AgregarAsync(EjercicioPersonalizado ejercicio)
    {
        context.EjerciciosPersonalizados.Add(ejercicio);
        await context.SaveChangesAsync();
    }
}
