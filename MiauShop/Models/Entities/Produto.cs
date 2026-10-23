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

    // Imagem do card: a marcada como principal; se não houver, a primeira.
    public string ImagemPrincipalUrl() =>
        Imagens.OrderByDescending(i => i.Principal).ThenBy(i => i.Ordem)
            .Select(i => i.Url).FirstOrDefault() ?? "/img/sem-imagem.webp";

    // Ex.: de R$ 50 por R$ 39 -> 22 (%)
    public int PercentualDesconto() =>
        EmPromocao() ? (int)Math.Round((1 - PrecoAtual() / Preco) * 100) : 0;

    // Controle de concorrência (coluna de sistema "xmin" do PostgreSQL), igual ao do Pedido.
    public uint Versao { get; private set; }

    // Regras que o cadastro precisa respeitar para o produto poder ser salvo.
    public IEnumerable<string> ValidarRegras()
    {
        if (Preco <= 0)
            yield return "O preço precisa ser maior que zero.";

        if (PrecoPromocional is decimal promocional && (promocional <= 0 || promocional >= Preco))
            yield return "O preço promocional precisa ser maior que zero e menor que o preço normal.";

        if (Ativo && !Variacoes.Any(v => v.Ativa))
            yield return "Um produto ativo precisa de pelo menos uma variação ativa.";
    }

    // Nova variação com SKU gerado a partir do slug (ex.: "moletom-pet-inverno-4").
    public ProdutoVariacao AdicionarVariacao(string nome, decimal ajustePreco)
    {
        var variacao = new ProdutoVariacao
        {
            Produto = this,
            Nome = nome.Trim(),
            AjustePreco = ajustePreco,
            Sku = $"{Slug}-{Variacoes.Count + 1}",
            Ordem = Variacoes.Count
        };

        Variacoes.Add(variacao);
        return variacao;
    }

    // Atualiza (ou cria) a imagem principal, usada no card e no detalhe.
    public void DefinirImagemPrincipal(string url, string textoAlternativo)
    {
        var principal = Imagens.FirstOrDefault(i => i.Principal);

        if (principal is null)
            Imagens.Add(new ProdutoImagem { Url = url, TextoAlternativo = textoAlternativo, Principal = true });
        else
        {
            principal.Url = url;
            principal.TextoAlternativo = textoAlternativo;
        }
    }
}
