using Microsoft.AspNetCore.Mvc;
using OverLoad.Models;

namespace OverLoad.Controllers;

public class TrackerController : Controller
{
    // LISTA ESTÁTICA: Actúa como nuestra base de datos temporal en memoria
    private static List<Ejercicio> _ejercicios = new List<Ejercicio>();
    private static int _contadorId = 1; // Para simular el Id autoincrementable

    // Mostrar la pantalla principal con la lista
    public IActionResult Index()
    {
        return View(_ejercicios);
    }

    // Recibe los datos del formulario y los guarda en la lista
    [HttpPost]
    public IActionResult Crear(Ejercicio nuevoEjercicio)
    {
        nuevoEjercicio.Id = _contadorId++; // Asignamos un ID único
        _ejercicios.Add(nuevoEjercicio);
        
        return RedirectToAction("Index"); // Recarga la página
    }

    // Actualiza las series y repeticiones del ejercicio existente
    [HttpPost]
    public IActionResult NuevaCarga(int id, int nuevasSeries, int nuevasRepeticiones)
    {
        // Buscamos el ejercicio en la lista por su ID
        var ejercicio = _ejercicios.FirstOrDefault(e => e.Id == id);
        
        if (ejercicio != null)
        {
            // Modificamos solo las series y repeticiones
            ejercicio.Series = nuevasSeries;
            ejercicio.Repeticiones = nuevasRepeticiones;
        }
        
        return RedirectToAction("Index");
    }
}