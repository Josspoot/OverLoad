using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OverLoad.Application.Libreria;
using OverLoad.Application.Nutricion;
using OverLoad.Application.Ports;
using OverLoad.Application.Progresion;
using OverLoad.Application.Progresion.Estrategias;
using OverLoad.Application.Services;
using OverLoad.Data;
using OverLoad.Infrastructure.Identidad;
using OverLoad.Infrastructure.OpenFoodFacts;
using OverLoad.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Acceso a la identidad del usuario autenticado sin acoplar el núcleo a ASP.NET.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();

// Persistencia del perfil metabólico (puente Calculadora -> Bitácora).
builder.Services.AddScoped<IPerfilMetabolicoRepository, EfPerfilMetabolicoRepository>();

// Arquitectura hexagonal: se enchufan los adaptadores a los puertos vía DI.
// El puerto de entrada (IEjercicioService) y el de salida (IEjercicioRepository)
// pueden cambiar de implementación sin tocar el núcleo ni los controladores.
// Patrón Decorator (ver ADR-04): el puerto de salida IEjercicioRepository se
// resuelve a un decorador de logging que envuelve al adaptador real (EF Core).
// El núcleo sigue dependiendo solo de IEjercicioRepository; el decorador es
// transparente y se puede quitar comentando estas líneas.
builder.Services.AddScoped<EfEjercicioRepository>();
builder.Services.AddScoped<IEjercicioRepository>(sp =>
    new EjercicioRepositoryLogDecorator(
        sp.GetRequiredService<EfEjercicioRepository>(),
        sp.GetRequiredService<ILogger<EjercicioRepositoryLogDecorator>>()));

builder.Services.AddScoped<IEjercicioService, EjercicioService>();

// Patrón Strategy (ver ADR-04): cada algoritmo de sobrecarga progresiva se
// registra como una implementación de IEstrategiaProgresion. Agregar una nueva
// estrategia solo requiere registrarla aquí; el selector las recibe todas por DI.
builder.Services.AddScoped<IEstrategiaProgresion, ProgresionPorPeso>();
builder.Services.AddScoped<IEstrategiaProgresion, ProgresionPorRepeticiones>();
builder.Services.AddScoped<IEstrategiaProgresion, ProgresionPorSeries>();
builder.Services.AddScoped<IEstrategiaProgresion, DobleProgresion>();
builder.Services.AddScoped<SelectorEstrategiaProgresion>();

// Servicio de dominio de la calculadora metabólica (lógica pura, sin persistencia).
builder.Services.AddScoped<CalculadoraMetabolica>();

// Cliente HTTP tipado para Open Food Facts (adaptador del puerto IBuscadorAlimentos).
// Open Food Facts pide un User-Agent descriptivo en cada petición.
builder.Services.AddHttpClient<IBuscadorAlimentos, OpenFoodFactsClient>(client =>
{
    client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("OverLoad/1.0 (proyecto academico)");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Catálogo de ejercicios de la librería (datos estáticos del dominio).
builder.Services.AddSingleton<CatalogoEjercicios>();

// Librería personalizada: catálogo base + ejercicios creados por el usuario.
builder.Services.AddScoped<IEjercicioPersonalizadoRepository, EfEjercicioPersonalizadoRepository>();
builder.Services.AddScoped<LibreriaService>();

// Documentacion de la API REST con OpenAPI / Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OverLoad API",
        Version = "v1",
        Description = "API REST para el seguimiento de entrenamientos de fuerza. " +
                      "Adaptador de entrada de la arquitectura hexagonal (ver ADR-03)."
    });

    // Incluye los comentarios XML para enriquecer la documentacion.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    // Swagger UI disponible en /swagger (solo en desarrollo).
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OverLoad API v1");
        options.DocumentTitle = "OverLoad API - Documentacion";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();