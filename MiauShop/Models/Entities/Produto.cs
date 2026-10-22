using MiauShop.Models.Enums;

namespace MiauShop.Models.Entities;

public class Produto : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string DescricaoCurta { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }

    public Animal Animal { get; set; }

    // Aparece na vitrine da página inicial.
    public bool Destaque { get; set; }

    // Exclusão lógica: produto já vendido nunca é apagado, só desativado.
    public bool Ativo { get; set; } = true;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; } = null!;

    // Todo produto tem pelo menos uma variação ("Único" quando não tem tamanho/cor).
    public ICollection<ProdutoVariacao> Variacoes { get; set; } = new List<ProdutoVariacao>();
    public ICollection<ProdutoImagem> Imagens { get; set; } = new List<ProdutoImagem>();

    // Promoção só vale se o preço promocional for realmente menor.
    public bool EmPromocao() =>
        PrecoPromocional is decimal promocional && promocional < Preco;

    public decimal PrecoAtual() =>
        EmPromocao() ? PrecoPromocional!.Value : Preco;

    // Ex.: de R$ 50 por R$ 39 -> 22 (%)
    public int PercentualDesconto() =>
        EmPromocao() ? (int)Math.Round((1 - PrecoAtual() / Preco) * 100) : 0;
}
