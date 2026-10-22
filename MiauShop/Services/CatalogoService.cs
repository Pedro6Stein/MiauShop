using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class CatalogoService(
    IProdutoRepositorio produtoRepositorio,
    IRepositorio<Categoria> categoriaRepositorio) : ICatalogoService
{
    public const int ItensPorPagina = 12;
    private const int QuantidadeDestaques = 8;
    private const int QuantidadeRelacionados = 4;

    public Task<List<Produto>> ListarDestaquesAsync() =>
        produtoRepositorio.ListarDestaquesAsync(QuantidadeDestaques);

    public Task<(List<Produto> Itens, int Total)> BuscarAsync(FiltroProdutos filtro)
    {
        filtro.Pagina = Math.Max(filtro.Pagina, 1);
        return produtoRepositorio.BuscarAsync(filtro, ItensPorPagina);
    }

    public Task<Produto?> ObterProdutoAsync(string slug) =>
        produtoRepositorio.ObterPorSlugAsync(slug);

    // Outros produtos da mesma categoria (destaques primeiro), sem repetir o atual.
    public async Task<List<Produto>> ListarRelacionadosAsync(Produto produto)
    {
        var filtro = new FiltroProdutos { Categoria = produto.Categoria.Slug };
        var (itens, _) = await produtoRepositorio.BuscarAsync(filtro, QuantidadeRelacionados + 1);

        return itens.Where(p => p.Id != produto.Id).Take(QuantidadeRelacionados).ToList();
    }

    public async Task<List<Categoria>> ListarCategoriasAsync() =>
        (await categoriaRepositorio.ListarAsync())
            .Where(c => c.Ativa)
            .OrderBy(c => c.Ordem)
            .ToList();
}
