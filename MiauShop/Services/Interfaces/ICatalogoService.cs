using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Services.Interfaces;

public interface ICatalogoService
{
    Task<List<Produto>> ListarDestaquesAsync();
    Task<(List<Produto> Itens, int Total)> BuscarAsync(Animal? animal, string? categoriaSlug, string? texto, int pagina);
    Task<Produto?> ObterProdutoAsync(string slug);
    Task<List<Categoria>> ListarCategoriasAsync();
}
