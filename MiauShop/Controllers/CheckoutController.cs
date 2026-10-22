using System.Text.Json;
using MiauShop.Models.ViewModels;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MiauShop.Controllers;

// Etapa 1: dados e endereço -> Etapa 2: pagamento (bloqueado) -> pedido criado -> confirmação.
public class CheckoutController(
    ICarrinhoService carrinhoService,
    ICheckoutService checkoutService,
    IConsultaCep consultaCep,
    IOptions<OpcoesMercadoPago> mercadoPago) : Controller
{
    private const string ChaveDados = "CheckoutDados";
    public const string ChaveUltimoPedido = "UltimoPedidoId";

    // GET /Checkout
    public async Task<IActionResult> Index()
    {
        var carrinho = await carrinhoService.ObterAsync();
        if (carrinho.Vazio)
            return RedirectToAction("Index", "Carrinho");

        return View(new CheckoutViewModel { Dados = LerDados() ?? new DadosCheckout(), Carrinho = carrinho });
    }

    // POST /Checkout: valida os dados no servidor e guarda na sessão até o pagamento.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index([Bind(Prefix = "Dados")] DadosCheckout dados)
    {
        var carrinho = await carrinhoService.ObterAsync();
        if (carrinho.Vazio)
            return RedirectToAction("Index", "Carrinho");

        // Com o CEP no formato certo, o servidor busca o endereço oficial antes de decidir se está tudo válido.
        if (ModelState.GetFieldValidationState("Dados.Cep") == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
            await ConfirmarEnderecoPeloCepAsync(dados);

        if (!ModelState.IsValid)
            return View(new CheckoutViewModel { Dados = dados, Carrinho = carrinho });

        HttpContext.Session.SetString(ChaveDados, JsonSerializer.Serialize(dados));
        return RedirectToAction(nameof(Pagamento));
    }

    // GET /Checkout/Pagamento
    public async Task<IActionResult> Pagamento()
    {
        var carrinho = await carrinhoService.ObterAsync();
        if (carrinho.Vazio)
            return RedirectToAction("Index", "Carrinho");

        var dados = LerDados();
        if (dados is null)
            return RedirectToAction(nameof(Index));

        return View(new CheckoutViewModel
        {
            Dados = dados,
            Carrinho = carrinho,
            PublicKeyMercadoPago = mercadoPago.Value.Configurado ? mercadoPago.Value.PublicKey : null
        });
    }

    // POST /Checkout/Finalizar: chamado depois do aviso "pagamento bloqueado".
    // Recebe só o MÉTODO escolhido; nenhum dado de cartão chega aqui.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(string? metodo)
    {
        var dados = LerDados();
        if (dados is null)
            return RedirectToAction(nameof(Index));

        var forma = MapeamentoPagamento.DoMercadoPago(metodo);
        if (forma is null)
        {
            TempData["Erro"] = "Escolha uma forma de pagamento.";
            return RedirectToAction(nameof(Pagamento));
        }

        try
        {
            var pedidoId = await checkoutService.FinalizarAsync(
                dados.Nome.Trim(), dados.Email, dados.Telefone.Trim(), dados.ParaEndereco(), forma.Value);

            HttpContext.Session.Remove(ChaveDados);
            HttpContext.Session.SetInt32(ChaveUltimoPedido, pedidoId);

            return RedirectToAction("Confirmacao", "Pedido", new { id = pedidoId });
        }
        catch (InvalidOperationException erro)
        {
            TempData["Erro"] = erro.Message;
            return RedirectToAction("Index", "Carrinho");
        }
    }

    // GET /Checkout/Cep?cep=01310100 -> endereço vindo do ViaCEP (consultado pelo servidor).
    [HttpGet]
    public async Task<IActionResult> Cep(string cep)
    {
        var endereco = await consultaCep.ConsultarAsync(cep);
        return endereco is null ? NotFound() : Json(endereco);
    }

    // O endereço não é "livre": o servidor consulta o CEP de novo e usa a cidade e a UF
    // oficiais (e rua/bairro, quando o CEP tem). O que o usuário digitou nesses campos é ignorado.
    private async Task ConfirmarEnderecoPeloCepAsync(DadosCheckout dados)
    {
        var oficial = await consultaCep.ConsultarAsync(dados.Cep);

        if (oficial is null)
        {
            ModelState.AddModelError("Dados.Cep", "Não encontramos esse CEP. Confira os números e tente de novo.");
            return;
        }

        dados.Cidade = oficial.Cidade;
        dados.Uf = oficial.Uf;
        AceitarValorOficial("Cidade", "Uf");

        // CEP "geral" de cidade pequena não traz rua/bairro: aí vale o que o cliente digitou.
        if (!string.IsNullOrWhiteSpace(oficial.Logradouro))
        {
            dados.Logradouro = oficial.Logradouro;
            AceitarValorOficial("Logradouro");
        }

        if (!string.IsNullOrWhiteSpace(oficial.Bairro))
        {
            dados.Bairro = oficial.Bairro;
            AceitarValorOficial("Bairro");
        }
    }

    // Tira do ModelState o que veio do formulário para esses campos (valor digitado e erros),
    // assim a tela e a validação passam a usar o valor oficial.
    private void AceitarValorOficial(params string[] campos)
    {
        foreach (var campo in campos)
            ModelState.Remove($"Dados.{campo}");
    }

    private DadosCheckout? LerDados()
    {
        var json = HttpContext.Session.GetString(ChaveDados);
        return json is null ? null : JsonSerializer.Deserialize<DadosCheckout>(json);
    }
}
