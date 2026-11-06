namespace MiauShop.Models.ViewModels;

// Uma resposta do assistente: balões de texto, produtos, links e botões de opção.
// O servidor também diz o que a PRÓXIMA mensagem do cliente vai significar (Etapa/Contexto),
// e o navegador só devolve esses valores na mensagem seguinte.
public class RespostaAssistente
{
    public List<string> Mensagens { get; } = [];
    public List<ProdutoNoAssistente> Produtos { get; } = [];
    public List<LinkNoAssistente> Links { get; } = [];
    public List<OpcaoAssistente> Opcoes { get; } = [];

    public string Etapa { get; set; } = "menu";
    public string? Contexto { get; set; }
    public string Dica { get; set; } = "Digite o número da opção ou sua dúvida";
}

public record OpcaoAssistente(string Valor, string Texto);
public record ProdutoNoAssistente(string Nome, string Url, decimal Preco, string ImagemUrl);
public record LinkNoAssistente(string Texto, string Url);
