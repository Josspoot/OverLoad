using System.Diagnostics;
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
public class HomeController(IEjercicioService ejercicios, CatalogoEjercicios catalogo) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Librería de ejercicios con sus fichas descriptivas.
    public IActionResult Privacy()
    {
        return View(catalogo.ObtenerGrupos());
    }

    public async Task<IActionResult> Tracker()
    {
        var lista = await ejercicios.ListarAsync();
        return View(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Ejercicio nuevoEjercicio)
    {
        await ejercicios.RegistrarAsync(nuevoEjercicio);
        return RedirectToAction("Tracker");
    }

    [HttpPost]
    public async Task<IActionResult> NuevaCarga(int id, int nuevasSeries, int nuevasRepeticiones, double nuevoPeso)
    {
        await ejercicios.ActualizarCargaAsync(id, nuevasSeries, nuevasRepeticiones, nuevoPeso);
        return RedirectToAction("Tracker");
    }

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
