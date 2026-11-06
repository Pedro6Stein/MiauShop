using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

// Tabela do frete por distância: CEP de origem (padrão: Bragança Paulista) e valores.
// O simulador é um GET, então o Visualizador também consegue testar.
public class FreteController(IFreteService frete) : AdminControllerBase
{
    // GET /Admin/Frete?simular=01310-100
    public async Task<IActionResult> Index(string? simular) =>
        View(await MontarAsync(formulario: null, simular));

    [HttpPost]
    public async Task<IActionResult> Salvar(FreteFormulario formulario)
    {
        if (!ModelState.IsValid)
            return View(nameof(Index), await MontarAsync(formulario, simular: null));

        var erros = await frete.SalvarConfiguracaoAsync(
            formulario.CepOrigem, formulario.ValorBase!.Value, formulario.ValorPorKm!.Value, formulario.ValorMaximo!.Value);

        if (erros.Count > 0)
        {
            erros.ForEach(erro => ModelState.AddModelError(string.Empty, erro));
            return View(nameof(Index), await MontarAsync(formulario, simular: null));
        }

        TempData["Sucesso"] = "Tabela de frete salva. Os próximos cálculos já usam os valores novos.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<FreteViewModel> MontarAsync(FreteFormulario? formulario, string? simular)
    {
        var configuracao = await frete.ObterConfiguracaoAsync();

        return new FreteViewModel
        {
            Formulario = formulario ?? new FreteFormulario
            {
                CepOrigem = configuracao.CepOrigem,
                ValorBase = configuracao.ValorBase,
                ValorPorKm = configuracao.ValorPorKm,
                ValorMaximo = configuracao.ValorMaximo
            },
            CidadeOrigem = configuracao.CidadeOrigem,
            UfOrigem = configuracao.UfOrigem,
            MinimoParaFreteGratis = frete.MinimoParaFreteGratis,
            CepSimulado = simular,
            // Subtotal 0: mostra o valor da tabela, sem o frete grátis.
            Simulacao = string.IsNullOrWhiteSpace(simular) ? null : await frete.CotarAsync(simular, 0)
        };
    }
}
