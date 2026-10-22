using MiauShop.Models.Entities;

namespace MiauShop.Models.ViewModels;

public class CarrinhoViewModel
{
    public required Carrinho Carrinho { get; init; }

    // Produtos sugeridos quando o carrinho está vazio.
    public List<Produto> Sugestoes { get; init; } = [];

    public int QuantidadeMaxima { get; init; }
}
