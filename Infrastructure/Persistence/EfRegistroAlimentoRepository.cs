using Microsoft.EntityFrameworkCore;
using OverLoad.Application.Ports;
using OverLoad.Data;
using OverLoad.Models;

namespace OverLoad.Infrastructure.Persistence;

/// <summary>Adaptador de salida (EF Core) del puerto <see cref="IRegistroAlimentoRepository"/>.</summary>
public class EfRegistroAlimentoRepository(ApplicationDbContext context) : IRegistroAlimentoRepository
{
    public async Task<IReadOnlyList<RegistroAlimento>> ObtenerDelDiaAsync(string userId, DateOnly fecha) =>
        await context.RegistrosAlimentos.AsNoTracking()
            .Where(r => r.UserId == userId && r.Fecha == fecha)
            .OrderBy(r => r.Id)
            .ToListAsync();

    public async Task AgregarAsync(RegistroAlimento registro)
    {
        context.RegistrosAlimentos.Add(registro);
        await context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id, string userId)
    {
        var registro = await context.RegistrosAlimentos.FindAsync(id);
        if (registro is null || registro.UserId != userId) return;

        context.RegistrosAlimentos.Remove(registro);
        await context.SaveChangesAsync();
    }
}
