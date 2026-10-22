namespace MiauShop.Models.Entities;

// Uma versão vendável do produto: "P", "M", "G", "Azul"... ou "Único".
// É sempre uma variação (e não o produto) que vai para o carrinho e para o pedido.
public class ProdutoVariacao : EntidadeBase
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;

    // Código único da variação (ex.: "MOL-INV-G")
    public string Sku { get; set; } = string.Empty;

    // Diferença em relação ao preço do produto (ex.: tamanho G custa +5,00).
    public decimal AjustePreco { get; set; }

    public int Ordem { get; set; }
    public bool Ativa { get; set; } = true;

    // Precisa do Produto carregado (Include) para funcionar.
    public decimal PrecoFinal() => Produto.PrecoAtual() + AjustePreco;
}
