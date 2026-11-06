using MiauShop.Models.Entities;

namespace MiauShop.Models.ViewModels;

public record CarrinhoViewModel
{
    public required Carrinho Carrinho { get; init; }

    // Produtos sugeridos quando o carrinho está vazio.
    public List<Produto> Sugestoes { get; init; } = [];

    public int QuantidadeMaxima { get; init; }

    // Mensagem do "Calcular frete" quando o CEP não é encontrado.
    public string? ErroFrete { get; init; }
}
