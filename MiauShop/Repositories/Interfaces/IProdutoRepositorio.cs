using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Repositories.Interfaces;

public interface IProdutoRepositorio : IRepositorio<Produto>
{
    Task<List<Produto>> ListarDestaquesAsync(int quantidade);

    // Busca da loja: todos os filtros são opcionais. Devolve a página e o total (para a paginação).
    Task<(List<Produto> Itens, int Total)> BuscarAsync(
        Animal? animal, string? categoriaSlug, string? texto, int pagina, int itensPorPagina);

    // Produto completo para a página de detalhe (categoria, fornecedor, variações e imagens).
    Task<Produto?> ObterPorSlugAsync(string slug);

    // Variações ativas com o produto e as imagens carregados (usado pelo carrinho e pelo checkout).
    Task<List<ProdutoVariacao>> ObterVariacoesAsync(IEnumerable<int> variacaoIds);
}
