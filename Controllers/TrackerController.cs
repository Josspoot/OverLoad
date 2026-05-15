using Microsoft.AspNetCore.Mvc;
using OverLoad.Models;
using System.Collections.Generic;
using System.Linq;

namespace OverLoad.Controllers;

public class TrackerController : Controller
{
    private static List<Ejercicio> _ejercicios = new List<Ejercicio>();
    private static int _contadorId = 1;

    public IActionResult Index()
    {
        return View(_ejercicios);
    }

    [HttpPost]
    public IActionResult Crear(Ejercicio nuevoEjercicio)
    {
        nuevoEjercicio.Id = _contadorId++;
        _ejercicios.Add(nuevoEjercicio);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult NuevaCarga(int id, int nuevasSeries, int nuevasRepeticiones)
    {
        var ejercicio = _ejercicios.FirstOrDefault(e => e.Id == id);
        if (ejercicio != null)
        {
            ejercicio.Series = nuevasSeries;
            ejercicio.Repeticiones = nuevasRepeticiones;
        }
        return RedirectToAction("Index");
    }

    // --- NUEVO MÉTODO PARA ELIMINAR ---
    [HttpPost]
    public IActionResult Eliminar(int id)
    {
        // Buscamos el ejercicio por su ID
        var ejercicio = _ejercicios.FirstOrDefault(e => e.Id == id);
        
        if (ejercicio != null)
        {
            // Si lo encuentra, lo removemos de la lista
            _ejercicios.Remove(ejercicio);
        }
        
        // Recargamos la página
        return RedirectToAction("Index");
    }
}