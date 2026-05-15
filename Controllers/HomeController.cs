using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OverLoad.Models;

namespace OverLoad.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // AGREGAMOS ESTE MÉTODO PARA CONECTAR TU VISTA
    public IActionResult Tracker() 
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}