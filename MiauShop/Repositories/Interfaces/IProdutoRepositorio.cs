using MiauShop.Models.Entities;
using MiauShop.Models;

namespace MiauShop.Repositories.Interfaces;

public interface IProdutoRepositorio : IRepositorio<Produto>
{
    Task<List<Produto>> ListarDestaquesAsync(int quantidade);

    // Busca da loja com filtros e ordenação. Devolve a página e o total (para a paginação).
    Task<(List<Produto> Itens, int Total)> BuscarAsync(FiltroProdutos filtro, int itensPorPagina);

    // Produto completo para a página de detalhe (categoria, fornecedor, variações e imagens).
    Task<Produto?> ObterPorSlugAsync(string slug);

    // Variações ativas com o produto e as imagens carregados (usado pelo carrinho e pelo checkout).
    Task<List<ProdutoVariacao>> ObterVariacoesAsync(IEnumerable<int> variacaoIds);

    // ---------- Painel (inclui produtos e variações inativos) ----------

    Task<(List<Produto> Itens, int Total)> ListarParaPainelAsync(string? busca, int? categoriaId, bool? ativo, int pagina, int itensPorPagina);

    // Produto com todas as variações e imagens, rastreado pelo EF para ser editado.
    Task<Produto?> ObterParaEdicaoAsync(int id);

    Task<List<string>> ListarSlugsAsync();
    Task<Dictionary<int, int>> ContarPorCategoriaAsync();
    Task<Dictionary<int, int>> ContarPorFornecedorAsync();
    Task<int> ContarAtivosDoFornecedorAsync(int fornecedorId);

    // Controle de concorrência: versão que o administrador tinha na tela.
    void DefinirVersaoOriginal(Produto produto, uint versao);
}
