namespace MiauShop.Models.Entities;

// "Foto" do item no momento da compra: nome e preço ficam copiados aqui,
// então mudar o produto depois não altera pedidos antigos.
public class ItemPedido : EntidadeBase
{
    // Construtor vazio usado pelo Entity Framework ao ler do banco.
    private ItemPedido() { }

    public int PedidoId { get; private set; }
    public Pedido Pedido { get; private set; } = null!;

    public int ProdutoVariacaoId { get; private set; }
    public ProdutoVariacao ProdutoVariacao { get; private set; } = null!;

    public string NomeProduto { get; private set; } = string.Empty;
    public string NomeVariacao { get; private set; } = string.Empty;
    public string ImagemUrl { get; private set; } = string.Empty;
    public decimal PrecoUnitario { get; private set; }
    public int Quantidade { get; private set; }

    public decimal Subtotal() => PrecoUnitario * Quantidade;

    // Única forma de criar um item: a partir de uma variação com o Produto carregado.
    public static ItemPedido CriarDe(ProdutoVariacao variacao, int quantidade, string imagemUrl)
    {
        if (quantidade < 1)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade mínima é 1.");

        return new ItemPedido
        {
            ProdutoVariacaoId = variacao.Id,
            NomeProduto = variacao.Produto.Nome,
            NomeVariacao = variacao.Nome,
            ImagemUrl = imagemUrl,
            PrecoUnitario = variacao.PrecoFinal(),
            Quantidade = quantidade
        };
    }
}
