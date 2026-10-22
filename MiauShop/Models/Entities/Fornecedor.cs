namespace MiauShop.Models.Entities;

// No dropshipping quem envia o produto é o fornecedor,
// por isso o prazo de envio pertence a ele e não ao produto.
public class Fornecedor : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public int PrazoEnvioDias { get; set; }
    public string? Contato { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
