using MiauShop.Areas.Admin.ViewModels;

namespace MiauShop.Services.Interfaces;

// Regras dos cadastros do painel. Os métodos de gravação devolvem a lista de erros (vazia = deu certo).
public interface IAdminCatalogoService
{
    Task<List<CategoriaLinha>> ListarCategoriasAsync();
    Task<CategoriaFormulario?> ObterCategoriaAsync(int id);
    Task<List<string>> SalvarCategoriaAsync(CategoriaFormulario formulario);
    Task<List<string>> ExcluirCategoriaAsync(int id);

    Task<List<FornecedorLinha>> ListarFornecedoresAsync();
    Task<FornecedorFormulario?> ObterFornecedorAsync(int id);
    Task<List<string>> SalvarFornecedorAsync(FornecedorFormulario formulario);
    Task<List<string>> ExcluirFornecedorAsync(int id);

    Task<ProdutosListaAdminViewModel> ListarProdutosAsync(string? busca, int? categoriaId, bool? ativo, int pagina);
    Task<ProdutoEdicaoViewModel?> ObterEdicaoProdutoAsync(int? id, ProdutoFormulario? formularioComErros = null);
    Task<(int Id, List<string> Erros)> SalvarProdutoAsync(ProdutoFormulario formulario);
    Task<List<string>> SalvarVariacaoAsync(int produtoId, VariacaoFormulario formulario);
}
