using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

public class ProdutosController(IAdminCatalogoService catalogo) : AdminControllerBase
{
    // GET /Admin/Produtos?busca=moletom&categoriaId=3&ativo=true&pagina=2
    public async Task<IActionResult> Index(string? busca, int? categoriaId, bool? ativo, int pagina = 1) =>
        View(await catalogo.ListarProdutosAsync(busca, categoriaId, ativo, pagina));

    public async Task<IActionResult> Novo() =>
        View("Formulario", await catalogo.ObterEdicaoProdutoAsync(null));

    public async Task<IActionResult> Editar(int id) =>
        await catalogo.ObterEdicaoProdutoAsync(id) is { } modelo ? View("Formulario", modelo) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Salvar([Bind(Prefix = "Formulario")] ProdutoFormulario formulario)
    {
        if (ModelState.IsValid)
        {
            var (id, erros) = await catalogo.SalvarProdutoAsync(formulario);
            if (erros.Count == 0)
            {
                TempData["Sucesso"] = $"Produto \"{formulario.Nome.Trim()}\" salvo.";
                return RedirectToAction(nameof(Editar), new { id });
            }

            erros.ForEach(erro => ModelState.AddModelError(string.Empty, erro));
        }

        var modelo = await catalogo.ObterEdicaoProdutoAsync(formulario.Id == 0 ? null : formulario.Id, formulario);
        return modelo is null ? NotFound() : View("Formulario", modelo);
    }

    // POST /Admin/Produtos/SalvarVariacao/12 (nova variação ou edição de uma existente)
    // Campos com prefixo "Variacao." para o Id da variação não se confundir com o id do produto (rota).
    [HttpPost]
    public async Task<IActionResult> SalvarVariacao([FromRoute] int id, [Bind(Prefix = "Variacao")] VariacaoFormulario variacao)
    {
        var erros = ModelState.IsValid
            ? await catalogo.SalvarVariacaoAsync(id, variacao)
            : ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

        if (erros.Count > 0) TempData["Erro"] = string.Join(" ", erros);
        else TempData["Sucesso"] = $"Variação \"{variacao.Nome.Trim()}\" salva.";

        return RedirectToAction(nameof(Editar), new { id });
    }
}
