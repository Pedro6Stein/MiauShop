using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Models.Enums;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

public class PedidosController(IAdminPedidoService adminPedidoService) : AdminControllerBase
{
    // GET /Admin/Pedidos?status=Enviado&busca=ana&pagina=2
    public async Task<IActionResult> Index(StatusPedido? status, string? busca, int pagina = 1) =>
        View(await adminPedidoService.ListarAsync(status, busca, pagina));

    // GET /Admin/Pedidos/Detalhe/12
    public async Task<IActionResult> Detalhe(int id)
    {
        var modelo = await adminPedidoService.ObterDetalheAsync(id);
        return modelo is null ? NotFound() : View(modelo);
    }

    // POST /Admin/Pedidos/AlterarStatus/12 (só papel Administrador: garantido no AdminControllerBase)
    [HttpPost]
    public async Task<IActionResult> AlterarStatus(int id, [Bind(Prefix = "Alteracao")] AlteracaoStatus alteracao)
    {
        var erro = ModelState.IsValid
            ? await adminPedidoService.AlterarStatusAsync(id, alteracao)
            : ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;

        if (erro is null)
            TempData["Sucesso"] = $"Pedido #{id} atualizado para \"{alteracao.NovoStatus!.Value.NomeExibicao()}\".";
        else
            TempData["Erro"] = erro;

        return RedirectToAction(nameof(Detalhe), new { id });
    }
}
