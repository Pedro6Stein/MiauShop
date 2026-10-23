using MiauShop.Models.Enums;
using MiauShop.Models.ViewModels;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MiauShop.Controllers;

public class PedidoController(
    IPedidoService pedidoService,
    Microsoft.Extensions.Options.IOptions<OpcoesAvancoAutomatico> avancoAutomatico) : Controller
{
    public const string PoliticaBusca = "busca-pedido";

    // GET /Pedido/Confirmacao/123
    // Só quem acabou de comprar (número guardado na própria sessão) vê esta página.
    public async Task<IActionResult> Confirmacao(int id)
    {
        if (HttpContext.Session.GetInt32(CheckoutController.ChaveUltimoPedido) != id)
            return NotFound();

        var pedido = await pedidoService.ObterAsync(id);
        return pedido is null ? NotFound() : View(pedido);
    }

    // GET /Pedido/Acompanhar?numero=123 -> só o formulário (o e-mail nunca vai na URL).
    [HttpGet]
    public IActionResult Acompanhar(int? numero, bool bloqueado = false) =>
        View(new AcompanhamentoViewModel { Busca = new BuscaPedido { Numero = numero }, Bloqueado = bloqueado });

    // POST /Pedido/Acompanhar -> busca por número + e-mail.
    // EnableRateLimiting: no máximo 10 buscas por minuto por visitante (evita "chutar" pedidos).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(PoliticaBusca)]
    public async Task<IActionResult> Acompanhar([Bind(Prefix = "Busca")] BuscaPedido busca)
    {
        if (!ModelState.IsValid)
            return View(new AcompanhamentoViewModel { Busca = busca });

        // Número errado ou e-mail errado dão a MESMA resposta: não revela quais pedidos existem.
        var pedido = await pedidoService.ObterParaAcompanhamentoAsync(busca.Numero!.Value, busca.Email);

        if (pedido is null)
            return View(new AcompanhamentoViewModel { Busca = busca, BuscaFeita = true });

        return View(new AcompanhamentoViewModel
        {
            Busca = busca,
            BuscaFeita = true,
            Pedido = pedido,
            Etapas = LinhaDoTempo.Montar(pedido),
            CriadoEm = LinhaDoTempo.ParaBrasilia(pedido.CriadoEm),
            CanceladoEm = LinhaDoTempo.DataDe(pedido, StatusPedido.Cancelado),
            MotivoCancelamento = pedido.Historico.FirstOrDefault(h => h.Status == StatusPedido.Cancelado)?.Observacao,
            AvancoAutomaticoHabilitado = avancoAutomatico.Value.Habilitado
        });
    }
}
