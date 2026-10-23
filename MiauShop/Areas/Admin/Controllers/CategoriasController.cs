using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

public class CategoriasController(IAdminCatalogoService catalogo) : AdminControllerBase
{
    public async Task<IActionResult> Index() => View(await catalogo.ListarCategoriasAsync());

    public IActionResult Nova() => View("Formulario", new CategoriaFormulario());

    public async Task<IActionResult> Editar(int id) =>
        await catalogo.ObterCategoriaAsync(id) is { } formulario ? View("Formulario", formulario) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Salvar(CategoriaFormulario formulario)
    {
        if (!ModelState.IsValid)
            return View("Formulario", formulario);

        var erros = await catalogo.SalvarCategoriaAsync(formulario);
        if (erros.Count > 0)
        {
            erros.ForEach(erro => ModelState.AddModelError(string.Empty, erro));
            return View("Formulario", formulario);
        }

        TempData["Sucesso"] = $"Categoria \"{formulario.Nome.Trim()}\" salva.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Excluir(int id)
    {
        var erros = await catalogo.ExcluirCategoriaAsync(id);
        if (erros.Count > 0) TempData["Erro"] = erros[0];
        else TempData["Sucesso"] = "Categoria excluída.";

        return RedirectToAction(nameof(Index));
    }
}
