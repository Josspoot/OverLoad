using Microsoft.AspNetCore.Mvc;
using OverLoad.Application.Nutricion;
using OverLoad.Models;

namespace OverLoad.Controllers;

/// <summary>
/// Adaptador de entrada (driving) de la calculadora metabólica. Traduce el
/// formulario HTTP en una llamada al servicio de dominio <see cref="CalculadoraMetabolica"/>
/// y devuelve la vista con el resultado. No contiene lógica de cálculo.
/// </summary>
public class CalculadoraController(CalculadoraMetabolica calculadora) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new CalculadoraViewModel());

    [HttpPost]
    public IActionResult Index(CalculadoraViewModel modelo)
    {
        var datos = new DatosMetabolicos(
            modelo.Sexo, modelo.Edad, modelo.PesoKg, modelo.AlturaCm,
            modelo.NivelActividad, modelo.Objetivo, modelo.Formula, modelo.CambioPesoKg);

        modelo.Resultado = calculadora.Calcular(datos);
        return View(modelo);
    }
}
