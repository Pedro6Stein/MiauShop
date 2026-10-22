using MiauShop.Models;
using MiauShop.Models.ViewModels;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Controllers;

[Route("Produtos")]
public class ProdutosController(ICatalogoService catalogoService) : Controller
{
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
    public async Task<IActionResult> Detalhe(string slug)
    {
        var produto = await catalogoService.ObterProdutoAsync(slug);

        if (produto is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NaoEncontrado");
        }

        var modelo = new ProdutoDetalheViewModel
        {
            Produto = produto,
            Relacionados = await catalogoService.ListarRelacionadosAsync(produto)
        };

        return View(modelo);
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
}
