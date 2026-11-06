using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.ViewModels;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MiauShop.Controllers;

[Route("Produtos")]
public class ProdutosController(ICatalogoService catalogoService, IAvaliacaoService avaliacaoService) : Controller
{
    public const string PoliticaAvaliacao = "avaliacao";

    // GET /Produtos?animal=Gato&categoria=brinquedos&ordem=MenorPreco ...
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] FiltroProdutos filtro)
    {
        var (produtos, total) = await catalogoService.BuscarAsync(filtro);

        var modelo = new CatalogoViewModel
        {
            Filtro = filtro,
            Produtos = produtos,
            Total = total,
            ItensPorPagina = CatalogoService.ItensPorPagina,
            Categorias = await catalogoService.ListarCategoriasAsync()
        };

        return View(modelo);
    }

    // GET /Produtos/moletom-pet-inverno
    [HttpGet("{slug}")]
    public async Task<IActionResult> Detalhe(string slug, bool bloqueado = false)
    {
        var produto = await catalogoService.ObterProdutoAsync(slug);

        if (produto is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NaoEncontrado");
        }

        if (bloqueado)
            ViewData["AvaliacaoAviso"] = "Muitas avaliações seguidas a partir da sua rede. Aguarde alguns minutos.";

        return View(await MontarDetalheAsync(produto, new NovaAvaliacao()));
    }

    // POST /Produtos/moletom-pet-inverno/avaliar
    // A avaliação entra pendente e só aparece depois de aprovada no painel.
    [HttpPost("{slug}/avaliar")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(PoliticaAvaliacao)]
    public async Task<IActionResult> Avaliar(string slug, [Bind(Prefix = "NovaAvaliacao")] NovaAvaliacao avaliacao)
    {
        var produto = await catalogoService.ObterProdutoAsync(slug);
        if (produto is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["AbrirFormularioAvaliacao"] = true;
            return View("Detalhe", await MontarDetalheAsync(produto, avaliacao));
        }

        await avaliacaoService.EnviarAsync(produto.Id, avaliacao);
        TempData["AvaliacaoEnviada"] = "Obrigado! Sua avaliação foi enviada e aparece aqui assim que a loja aprovar.";
        return Redirect($"/Produtos/{produto.Slug}#avaliacoes");
    }

    // GET /Produtos/moletom-pet-inverno/escolher
    // Conteúdo da janelinha de tamanho aberta pelo "+" do card.
    [HttpGet("{slug}/escolher")]
    public async Task<IActionResult> Escolher(string slug)
    {
        var produto = await catalogoService.ObterProdutoAsync(slug);
        return produto is null ? NotFound() : PartialView("_EscolherTamanho", produto);
    }

    // GET /Produtos/moletom-pet-inverno/preco?variacaoId=7
    // Devolve só o bloco de preço já renderizado (usado ao trocar o tamanho na página do produto).
    [HttpGet("{slug}/preco")]
    public async Task<IActionResult> Preco(string slug, int variacaoId)
    {
        var produto = await catalogoService.ObterProdutoAsync(slug);
        var variacao = produto?.Variacoes.FirstOrDefault(v => v.Id == variacaoId);

        return variacao is null ? NotFound() : PartialView("_Preco", variacao);
    }

    private async Task<ProdutoDetalheViewModel> MontarDetalheAsync(Produto produto, NovaAvaliacao novaAvaliacao) => new()
    {
        Produto = produto,
        Relacionados = await catalogoService.ListarRelacionadosAsync(produto),
        Avaliacoes = await avaliacaoService.ObterResumoAsync(produto.Id),
        NovaAvaliacao = novaAvaliacao
    };
}
