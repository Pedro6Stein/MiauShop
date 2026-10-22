using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Controllers;

public class CarrinhoController(ICarrinhoService carrinhoService) : Controller
{
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
}
