using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

// Moderação das avaliações dos clientes: aprovar (aparece na loja) ou remover.
// Herdando do AdminControllerBase, o Visualizador vê a fila mas não consegue aprovar nem remover.
public class AvaliacoesController(IAvaliacaoService avaliacoes) : AdminControllerBase
{
    // GET /Admin/Avaliacoes?situacao=pendentes|aprovadas|todas
    public async Task<IActionResult> Index(string situacao = AvaliacoesViewModel.Pendentes)
    {
        bool? aprovada = situacao switch
        {
            AvaliacoesViewModel.Aprovadas => true,
            AvaliacoesViewModel.Todas => null,
            _ => false
        };

        return View(new AvaliacoesViewModel
        {
            Situacao = aprovada switch { true => AvaliacoesViewModel.Aprovadas, null => AvaliacoesViewModel.Todas, _ => AvaliacoesViewModel.Pendentes },
            Avaliacoes = await avaliacoes.ListarParaPainelAsync(aprovada),
            QuantidadePendentes = await avaliacoes.ContarPendentesAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Aprovar(int id, string situacao = AvaliacoesViewModel.Pendentes)
    {
        if (await avaliacoes.AprovarAsync(id)) TempData["Sucesso"] = "Avaliação aprovada: já aparece na página do produto.";
        else TempData["Erro"] = "Avaliação não encontrada.";

        return RedirectToAction(nameof(Index), new { situacao });
    }

    [HttpPost]
    public async Task<IActionResult> Remover(int id, string situacao = AvaliacoesViewModel.Pendentes)
    {
        if (await avaliacoes.RemoverAsync(id)) TempData["Sucesso"] = "Avaliação removida.";
        else TempData["Erro"] = "Avaliação não encontrada.";

        return RedirectToAction(nameof(Index), new { situacao });
    }
}
