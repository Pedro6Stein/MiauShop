using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiauShop.Models.ViewModels;
using MiauShop.Services.Interfaces;

namespace MiauShop.Controllers;

public class HomeController(ICatalogoService catalogoService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var modelo = new HomeViewModel
        {
            Categorias = await catalogoService.ListarCategoriasAsync(),
            Destaques = await catalogoService.ListarDestaquesAsync()
        };

        return View(modelo);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
