using Microsoft.AspNetCore.Mvc;
using OverLoad.Application.Nutricion;
using OverLoad.Application.Ports;
using OverLoad.Models;

namespace OverLoad.Controllers;

/// <summary>
/// Adaptador de entrada (driving) de la calculadora metabólica. Traduce el
/// formulario HTTP en una llamada al servicio de dominio <see cref="CalculadoraMetabolica"/>
/// y devuelve la vista con el resultado. Si el usuario está autenticado, guarda el
/// resultado como su perfil metabólico activo para que la Bitácora lo utilice.
/// </summary>
public class CalculadoraController(
    CalculadoraMetabolica calculadora,
    IPerfilMetabolicoRepository perfiles,
    IUsuarioActual usuario) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new CalculadoraViewModel());

    [HttpPost]
    public async Task<IActionResult> Index(CalculadoraViewModel modelo)
    {
        var datos = new DatosMetabolicos(
            modelo.Sexo, modelo.Edad, modelo.PesoKg, modelo.AlturaCm,
            modelo.NivelActividad, modelo.Objetivo, modelo.Formula, modelo.CambioPesoKg);

        var resultado = calculadora.Calcular(datos);
        modelo.Resultado = resultado;

        // Persiste el perfil activo del usuario autenticado (puente a la Bitácora).
        if (usuario.Autenticado)
        {
            await perfiles.GuardarAsync(new PerfilMetabolico
            {
                UserId = usuario.Id!,
                Actualizado = DateTime.UtcNow,
                Sexo = datos.Sexo,
                Edad = datos.Edad,
                PesoKg = datos.PesoKg,
                AlturaCm = datos.AlturaCm,
                NivelActividad = datos.NivelActividad,
                Objetivo = datos.Objetivo,
                Formula = datos.Formula,
                Tmb = resultado.Tmb,
                Tdee = resultado.Tdee,
                CaloriasObjetivo = resultado.CaloriasObjetivo,
                ProteinaG = resultado.Macros.ProteinaG,
                CarbohidratoG = resultado.Macros.CarbohidratoG,
                GrasaG = resultado.Macros.GrasaG,
                NombreObjetivo = resultado.NombreObjetivo
            });
            modelo.PerfilGuardado = true;
        }

        return View(modelo);
    }
}
