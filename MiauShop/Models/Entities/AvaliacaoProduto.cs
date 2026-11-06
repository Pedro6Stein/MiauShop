namespace MiauShop.Models.Entities;

// Avaliação deixada por um cliente na página do produto.
// Entra como pendente e só aparece na loja depois que um administrador aprova no painel
// (o site é público: sem moderação, qualquer texto apareceria para todo mundo).
public class AvaliacaoProduto : EntidadeBase
{
    public const int NotaMinima = 1;
    public const int NotaMaxima = 5;
    public const int TamanhoMaximoNome = 60;
    public const int TamanhoMaximoComentario = 600;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;

    public bool Aprovada { get; private set; }

    public void Aprovar() => Aprovada = true;

    public static AvaliacaoProduto Nova(int produtoId, string nome, int nota, string comentario)
    {
        if (nota is < NotaMinima or > NotaMaxima)
            throw new ArgumentOutOfRangeException(nameof(nota), $"A nota vai de {NotaMinima} a {NotaMaxima}.");

        return new AvaliacaoProduto
        {
            ProdutoId = produtoId,
            Nome = nome.Trim(),
            Nota = nota,
            Comentario = comentario.Trim()
        };
    }

    // Nome exibido na loja: primeiro nome + inicial do sobrenome ("Ana Souza" -> "Ana S.").
    public string NomeExibicao()
    {
        var partes = Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length switch
        {
            0 => "Cliente",
            1 => partes[0],
            _ => $"{partes[0]} {char.ToUpperInvariant(partes[^1][0])}."
        };
    }
}
