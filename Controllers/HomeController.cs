using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverLoad.Application.Libreria;
using OverLoad.Application.Ports;
using OverLoad.Models;

namespace OverLoad.Controllers;

/// <summary>
/// Adaptador de entrada (driving). Traduce las peticiones HTTP en llamadas a
/// los casos de uso del núcleo a través del puerto IEjercicioService.
/// No conoce EF Core ni la base de datos.
/// </summary>
public class HomeController(IEjercicioService ejercicios, LibreriaService libreria) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Librería de ejercicios: catálogo base + ejercicios personalizados del usuario.
    public async Task<IActionResult> Privacy()
    {
        return View(await libreria.ObtenerGruposAsync());
    }

    [Authorize]
    public async Task<IActionResult> Tracker()
    {
        var lista = await ejercicios.ListarAsync();
        return View(lista);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Crear(Ejercicio nuevoEjercicio)
    {
        await ejercicios.RegistrarAsync(nuevoEjercicio);
        return RedirectToAction("Tracker");
    }

    // Crea una entrada de Tracker a partir de un ejercicio de la Librería.
    // Se registra con cargas por defecto que el usuario ajusta luego.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarDesdeLibreria(string nombre, string grupo)
    {
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            await ejercicios.RegistrarAsync(new Ejercicio
            {
                Nombre = nombre,
                Enfoque = string.IsNullOrWhiteSpace(grupo) ? "General" : grupo,
                Series = 3,
                Repeticiones = 10,
                Peso = 0,
                Esfuerzo = 5
            });
            TempData["TrackerMensaje"] = $"«{nombre}» se añadió a tu Tracker. Ajusta series, reps y peso.";
        }

        return RedirectToAction("Tracker");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> NuevaCarga(int id, int nuevasSeries, int nuevasRepeticiones, double nuevoPeso)
    {
        await ejercicios.ActualizarCargaAsync(id, nuevasSeries, nuevasRepeticiones, nuevoPeso);
        return RedirectToAction("Tracker");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        await ejercicios.EliminarAsync(id);
        return RedirectToAction("Tracker");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
