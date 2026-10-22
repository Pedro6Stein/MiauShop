using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Controllers;

public class PedidoController(IPedidoService pedidoService) : Controller
{
    // GET /Pedido/Confirmacao/123
    // Só quem acabou de comprar (número guardado na própria sessão) vê esta página.
    public async Task<IActionResult> Confirmacao(int id)
    {
        if (HttpContext.Session.GetInt32(CheckoutController.ChaveUltimoPedido) != id)
            return NotFound();

        var pedido = await pedidoService.ObterAsync(id);
        return pedido is null ? NotFound() : View(pedido);
    }
}
