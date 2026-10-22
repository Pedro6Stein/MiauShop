using MiauShop.Models.Entities;

namespace MiauShop.Models.ViewModels;

public class ProdutoDetalheViewModel
{
    public required Produto Produto { get; init; }
    public required List<Produto> Relacionados { get; init; }

    // Variações ativas na ordem definida no cadastro.
    public List<ProdutoVariacao> Variacoes =>
        Produto.Variacoes.OrderBy(v => v.Ordem).ToList();

    // "Único" não precisa de seletor de tamanho.
    public bool TemEscolhaDeVariacao => Variacoes.Count > 1;
}
