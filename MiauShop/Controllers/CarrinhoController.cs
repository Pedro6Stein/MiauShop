using MiauShop.Models.ViewModels;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Controllers;

public class CarrinhoController(ICarrinhoService carrinhoService, ICatalogoService catalogoService) : Controller
{
    private const int QuantidadeSugestoes = 4;

    // GET /Carrinho
    public async Task<IActionResult> Index() => View(await MontarModeloAsync());

    // POST /Carrinho/Adicionar
    // ValidateAntiForgeryToken: só aceita o envio vindo de um formulário da própria loja.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(int variacaoId, int quantidade = 1)
    {
        var querJson = Request.Headers.Accept.ToString().Contains("application/json");

        try
        {
            await carrinhoService.AdicionarAsync(variacaoId, quantidade);
        }
        catch (InvalidOperationException erro)
        {
            if (querJson)
                return BadRequest(new { erro = erro.Message });

            TempData["Erro"] = erro.Message;
            return Redirect(Request.Headers.Referer.ToString() is { Length: > 0 } origem ? origem : "/");
        }

        // Pelo JavaScript: devolve o novo total para atualizar o ícone do carrinho.
        // Sem JavaScript (formulário comum): vai para o carrinho.
        return querJson
            ? Json(new { quantidade = carrinhoService.ContarItens() })
            : Redirect("/Carrinho");
    }

    // POST /Carrinho/AlterarQuantidade (botões − e + de cada item)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarQuantidade(int variacaoId, int quantidade)
    {
        carrinhoService.AlterarQuantidade(variacaoId, quantidade);
        return await RespostaAsync();
    }

    // POST /Carrinho/Remover
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(int variacaoId)
    {
        carrinhoService.Remover(variacaoId);
        return await RespostaAsync();
    }

    // Pelo JavaScript (cabeçalho X-Parcial): devolve só o conteúdo do carrinho já renderizado.
    // Sem JavaScript: recarrega a página do carrinho.
    private async Task<IActionResult> RespostaAsync() =>
        Request.Headers.ContainsKey("X-Parcial")
            ? PartialView("_Conteudo", await MontarModeloAsync())
            : RedirectToAction(nameof(Index));

    private async Task<CarrinhoViewModel> MontarModeloAsync()
    {
        var carrinho = await carrinhoService.ObterAsync();

        return new CarrinhoViewModel
        {
            Carrinho = carrinho,
            QuantidadeMaxima = CarrinhoService.QuantidadeMaxima,
            Sugestoes = carrinho.Vazio
                ? (await catalogoService.ListarDestaquesAsync()).Take(QuantidadeSugestoes).ToList()
                : []
        };
    }
}
