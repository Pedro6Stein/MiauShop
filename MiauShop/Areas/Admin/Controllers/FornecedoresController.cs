using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

public class FornecedoresController(IAdminCatalogoService catalogo) : AdminControllerBase
{
    public async Task<IActionResult> Index() => View(await catalogo.ListarFornecedoresAsync());

    public IActionResult Novo() => View("Formulario", new FornecedorFormulario());

    public async Task<IActionResult> Editar(int id) =>
        await catalogo.ObterFornecedorAsync(id) is { } formulario ? View("Formulario", formulario) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Salvar(FornecedorFormulario formulario)
    {
        if (!ModelState.IsValid)
            return View("Formulario", formulario);

        var erros = await catalogo.SalvarFornecedorAsync(formulario);
        if (erros.Count > 0)
        {
            erros.ForEach(erro => ModelState.AddModelError(string.Empty, erro));
            return View("Formulario", formulario);
        }

        TempData["Sucesso"] = $"Fornecedor \"{formulario.Nome.Trim()}\" salvo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Excluir(int id)
    {
        var erros = await catalogo.ExcluirFornecedorAsync(id);
        if (erros.Count > 0) TempData["Erro"] = erros[0];
        else TempData["Sucesso"] = "Fornecedor excluído.";

        return RedirectToAction(nameof(Index));
    }
}
