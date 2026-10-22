using MiauShop.Models;
using MiauShop.Models.Entities;

namespace MiauShop.Services.Interfaces;

public interface ICatalogoService
{
    Task<List<Produto>> ListarDestaquesAsync();
    Task<(List<Produto> Itens, int Total)> BuscarAsync(FiltroProdutos filtro);
    Task<Produto?> ObterProdutoAsync(string slug);
    Task<List<Produto>> ListarRelacionadosAsync(Produto produto);
    Task<List<Categoria>> ListarCategoriasAsync();
}
