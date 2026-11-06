using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MiauShop.Models.Entities;

namespace MiauShop.Services;

// O "cérebro" do assistente: sem IA, só regras. Compara as palavras da mensagem do cliente
// com as palavras-chave e a pergunta de cada resposta cadastrada e escolhe a de maior pontuação.
public static partial class BuscadorDeRespostas
{
    // Palavra-chave encontrada vale 2 pontos; palavra da própria pergunta vale 1.
    private const int PontosPalavraChave = 2;
    private const int PontosPalavraDaPergunta = 1;
    public const int PontuacaoMinima = 2;

    // Palavras comuns que não ajudam a entender o assunto.
    private static readonly HashSet<string> PalavrasVazias =
    [
        "para", "como", "qual", "quais", "quanto", "quando", "onde", "porque", "voces", "voce", "sobre",
        "minha", "meu", "meus", "minhas", "esse", "essa", "isso", "este", "esta", "pelo", "pela", "tenho",
        "posso", "pode", "fazer", "seria", "gostaria", "queria", "saber", "preciso", "loja", "miaushop"
    ];

    // minúsculas, sem acento e só letras/números: "Ração p/ Gatos!" -> "racao p gatos"
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var semAcento = new StringBuilder();
        foreach (var letra in texto.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)
                semAcento.Append(char.IsLetterOrDigit(letra) ? letra : ' ');

        return EspacosRepetidos().Replace(semAcento.ToString(), " ").Trim();
    }

    // Palavras que valem para busca (4+ letras e fora da lista de palavras comuns).
    public static List<string> PalavrasImportantes(string? texto) =>
        Normalizar(texto).Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Length >= 4 && !PalavrasVazias.Contains(p))
            .Distinct()
            .ToList();

    public static int Pontuar(string mensagem, PerguntaFrequente pergunta)
    {
        // Espaço antes de cada termo: "troca" casa com "trocar" e "troca", mas não com "destroca".
        var texto = $" {Normalizar(mensagem)} ";
        var pontos = 0;

        foreach (var chave in pergunta.PalavrasChave.Split(',').Select(Normalizar).Where(c => c.Length > 0).Distinct())
            if (texto.Contains($" {chave}"))
                pontos += PontosPalavraChave;

        foreach (var palavra in PalavrasImportantes(pergunta.Pergunta))
            if (texto.Contains($" {palavra}"))
                pontos += PontosPalavraDaPergunta;

        return pontos;
    }

    // A resposta mais parecida com a mensagem, ou null se nenhuma chegar à pontuação mínima.
    // Empate: vale a que vem primeiro na lista do painel.
    public static PerguntaFrequente? MelhorResposta(string mensagem, IEnumerable<PerguntaFrequente> perguntas) =>
        perguntas
            .Select(p => (Pergunta: p, Pontos: Pontuar(mensagem, p)))
            .Where(x => x.Pontos >= PontuacaoMinima)
            .OrderByDescending(x => x.Pontos)
            .ThenBy(x => x.Pergunta.Ordem)
            .ThenBy(x => x.Pergunta.Id)
            .Select(x => x.Pergunta)
            .FirstOrDefault();

    // CEP escrito no meio da frase ("quanto fica pro 01310-100?").
    public static string? EncontrarCep(string? texto) =>
        texto is not null && Cep().Match(texto) is { Success: true } achado
            ? achado.Value.Replace("-", "").Replace(".", "")
            : null;

    // Pedidos que levam direto a uma das ferramentas do assistente.
    public static string? IdentificarAcao(string? mensagem)
    {
        var texto = $" {Normalizar(mensagem)} ";
        if (new[] { " rastre", " meu pedido", " status do pedido", " acompanhar", " onde esta meu", " cade meu" }.Any(texto.Contains))
            return EtapasAssistente.PedidoNumero;
        if (new[] { " atendente", " humano", " falar com", " whatsapp", " telefone", " contato" }.Any(texto.Contains))
            return EtapasAssistente.Atendente;
        if (new[] { " calcular frete", " valor do frete", " quanto custa o frete", " quanto e o frete", " frete para", " cep" }.Any(texto.Contains))
            return EtapasAssistente.Frete;
        return null;
    }

    private static readonly string[] Cumprimentos = ["oi", "ola", "oie", "bom dia", "boa tarde", "boa noite", "hello", "eai", "e ai", "tudo bem"];
    private static readonly string[] Agradecimentos = ["obrigad", "valeu", "brigad", "agradec"];

    public static Cortesia? IdentificarCortesia(string? mensagem)
    {
        var texto = Normalizar(mensagem);
        if (Agradecimentos.Any(a => $" {texto}".Contains($" {a}")))
            return Cortesia.Agradecimento;
        // Só a saudação sozinha ("oi", "bom dia!"): "oi, vocês têm ração?" segue para a busca.
        if (Cumprimentos.Any(c => texto == c || texto == $"{c} tudo bem" || texto == $"{c} miau"))
            return Cortesia.Cumprimento;
        return null;
    }

    // E-mail, ou 6+ números seguidos (telefone, CPF, cartão): não vai para a lista do painel.
    public static bool PareceDadoPessoal(string texto) =>
        texto.Contains('@') || MuitosNumeros().IsMatch(texto);

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosRepetidos();

    [GeneratedRegex(@"\b\d{2}\.?\d{3}-?\d{3}\b")]
    private static partial Regex Cep();

    [GeneratedRegex(@"\d[\d.\-\s]{5,}\d")]
    private static partial Regex MuitosNumeros();
}

public enum Cortesia { Cumprimento, Agradecimento }

// O que a próxima mensagem do cliente significa (vai e volta entre o navegador e o servidor).
public static class EtapasAssistente
{
    public const string Inicio = "inicio";
    public const string Menu = "menu";
    public const string Duvidas = "duvidas";
    public const string Produto = "produto";
    public const string Frete = "frete";
    public const string PedidoNumero = "pedido-numero";
    public const string PedidoEmail = "pedido-email";
    public const string Atendente = "atendente";
}
