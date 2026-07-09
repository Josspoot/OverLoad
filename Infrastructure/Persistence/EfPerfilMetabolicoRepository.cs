using Microsoft.EntityFrameworkCore;
using OverLoad.Application.Ports;
using OverLoad.Data;
using OverLoad.Models;

namespace OverLoad.Infrastructure.Persistence;

/// <summary>
/// Adaptador de salida (EF Core) del puerto <see cref="IPerfilMetabolicoRepository"/>.
/// Guarda un único perfil por usuario: si ya existe, lo actualiza (upsert).
/// </summary>
public class EfPerfilMetabolicoRepository(ApplicationDbContext context) : IPerfilMetabolicoRepository
{
    public async Task<PerfilMetabolico?> ObtenerAsync(string userId) =>
        await context.PerfilesMetabolicos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task GuardarAsync(PerfilMetabolico perfil)
    {
        var existente = await context.PerfilesMetabolicos
            .FirstOrDefaultAsync(p => p.UserId == perfil.UserId);

        if (existente is null)
        {
            context.PerfilesMetabolicos.Add(perfil);
        }
        else
        {
            perfil.Id = existente.Id;
            context.Entry(existente).CurrentValues.SetValues(perfil);
        }

        await context.SaveChangesAsync();
    }
}
