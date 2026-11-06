namespace MiauShop.Models.Entities;

// Pergunta e resposta do assistente da loja, cadastradas no painel (Admin > Assistente).
// Palavras-chave: termos separados por vírgula que, se aparecerem na mensagem do cliente,
// levam a esta resposta (ex.: "troca, trocar, devolver").
public class PerguntaFrequente : EntidadeBase
{
    public const int TamanhoMaximoPergunta = 150;
    public const int TamanhoMaximoResposta = 1000;
    public const int TamanhoMaximoPalavrasChave = 300;

    public string Pergunta { get; set; } = string.Empty;
    public string Resposta { get; set; } = string.Empty;
    public string PalavrasChave { get; set; } = string.Empty;

    // Posição na lista "Dúvidas frequentes" do assistente.
    public int Ordem { get; set; }
    public bool Ativa { get; set; } = true;
}

// Mensagem que o assistente não soube responder. Fica no painel para a loja criar uma resposta.
// Mensagens iguais (ignorando acentos e maiúsculas) viram uma linha só, com a contagem de vezes.
public class PerguntaSemResposta : EntidadeBase
{
    public const int TamanhoMaximoTexto = 200;

    public string Texto { get; set; } = string.Empty;
    public string TextoNormalizado { get; set; } = string.Empty;
    public int Vezes { get; set; } = 1;
}
