using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverLoad.Application.Nutricion;
using OverLoad.Application.Ports;
using OverLoad.Models;

namespace OverLoad.Controllers;

/// <summary>
/// Espacio "Bitácora": registro diario de alimentos. Muestra las calorías
/// consumidas del día frente al objetivo del perfil metabólico (tomado de la
/// Calculadora) y permite buscar alimentos en Open Food Facts para registrarlos.
/// Requiere sesión iniciada porque los registros pertenecen al usuario.
/// </summary>
[Authorize]
public class BitacoraController(
    IRegistroAlimentoRepository registros,
    IPerfilMetabolicoRepository perfiles,
    IBuscadorAlimentos buscador,
    IUsuarioActual usuario) : Controller
{
    // Panel del día: registros, totales y barras de progreso contra el objetivo.
    public async Task<IActionResult> Index(DateOnly? fecha)
    {
        var dia = fecha ?? DateOnly.FromDateTime(DateTime.Now);
        var delDia = await registros.ObtenerDelDiaAsync(usuario.Id!, dia);
        var perfil = await perfiles.ObtenerAsync(usuario.Id!);

        return View(new BitacoraViewModel
        {
            Fecha = dia,
            Perfil = perfil,
            Registros = delDia,
            CaloriasConsumidas = Math.Round(delDia.Sum(r => r.Calorias)),
            ProteinaConsumida = Math.Round(delDia.Sum(r => r.Proteina)),
            CarboConsumido = Math.Round(delDia.Sum(r => r.Carbohidrato)),
            GrasaConsumida = Math.Round(delDia.Sum(r => r.Grasa))
        });
    }

    // Sección conectada a Open Food Facts para buscar y elegir alimentos.
    public async Task<IActionResult> Buscar(string? q)
    {
        var modelo = new BuscarAlimentoViewModel { Termino = q ?? string.Empty };

        if (!string.IsNullOrWhiteSpace(q))
        {
            modelo.Resultados = await buscador.BuscarAsync(q);
            modelo.Buscado = true;
        }

        return View(modelo);
    }

    // Registra el alimento elegido escalando sus valores a los gramos consumidos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(AgregarAlimentoInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre) || input.Gramos <= 0)
            return RedirectToAction(nameof(Buscar));

        var factor = input.Gramos / 100.0;

        await registros.AgregarAsync(new RegistroAlimento
        {
            UserId = usuario.Id!,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Nombre = input.Nombre,
            Marca = input.Marca,
            CodigoBarras = input.CodigoBarras,
            Gramos = input.Gramos,
            Calorias = Math.Round(input.CaloriasPor100g * factor, 1),
            Proteina = Math.Round(input.ProteinaPor100g * factor, 1),
            Carbohidrato = Math.Round(input.CarboPor100g * factor, 1),
            Grasa = Math.Round(input.GrasaPor100g * factor, 1)
        });

        TempData["BitacoraMensaje"] = $"«{input.Nombre}» ({input.Gramos:0} g) se registró en tu Bitácora.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await registros.EliminarAsync(id, usuario.Id!);
        return RedirectToAction(nameof(Index));
    }
}
