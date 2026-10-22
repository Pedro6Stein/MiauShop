using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class CatalogoService(
    IProdutoRepositorio produtoRepositorio,
    IRepositorio<Categoria> categoriaRepositorio) : ICatalogoService
{
    public const int ItensPorPagina = 12;
    private const int QuantidadeDestaques = 8;

    public Task<List<Produto>> ListarDestaquesAsync() =>
        produtoRepositorio.ListarDestaquesAsync(QuantidadeDestaques);

    public Task<(List<Produto> Itens, int Total)> BuscarAsync(
        Animal? animal, string? categoriaSlug, string? texto, int pagina) =>
        produtoRepositorio.BuscarAsync(animal, categoriaSlug, texto, Math.Max(pagina, 1), ItensPorPagina);

    public Task<Produto?> ObterProdutoAsync(string slug) =>
        produtoRepositorio.ObterPorSlugAsync(slug);

    public async Task<List<Categoria>> ListarCategoriasAsync() =>
        (await categoriaRepositorio.ListarAsync())
            .Where(c => c.Ativa)
            .OrderBy(c => c.Ordem)
            .ToList();
}
