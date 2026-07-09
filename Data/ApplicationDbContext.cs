using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OverLoad.Models;

namespace OverLoad.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
    public DbSet<PerfilMetabolico> PerfilesMetabolicos => Set<PerfilMetabolico>();
    public DbSet<EjercicioPersonalizado> EjerciciosPersonalizados => Set<EjercicioPersonalizado>();
    public DbSet<RegistroAlimento> RegistrosAlimentos => Set<RegistroAlimento>();
}