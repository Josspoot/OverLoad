using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverLoad.Application.Libreria;
using OverLoad.Models;

namespace OverLoad.Controllers;

/// <summary>
/// Adaptador de entrada para gestionar los ejercicios personalizados de la
/// librería. Requiere sesión iniciada porque los ejercicios pertenecen al usuario.
/// </summary>
[Authorize]
public class LibreriaController(LibreriaService libreria) : Controller
{
    [HttpGet]
    public IActionResult Nueva() => View(new NuevoEjercicioViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(NuevoEjercicioViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        await libreria.AgregarAsync(
            modelo.Nombre, modelo.Grupo, modelo.ComoHacerlo,
            modelo.QueSeDebeSentir, modelo.Recomendaciones, modelo.EquipoExtra);

        TempData["LibreriaMensaje"] = $"«{modelo.Nombre}» se añadió a tu librería.";
        return RedirectToAction("Privacy", "Home");
    }
}
