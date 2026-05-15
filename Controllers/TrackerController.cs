using Microsoft.AspNetCore.Mvc;

namespace OverLoad.Controllers;

public class TrackerController : Controller
{
    // El método Index es el que se conecta con tu archivo Index.cshtml
    public IActionResult Index()
    {
        return View();
    }
}