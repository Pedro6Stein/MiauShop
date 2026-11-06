using System.Text.Json;
using MiauShop.Models;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

// A sessão guarda só [variacaoId, quantidade]. Preço nunca fica na sessão:
// é sempre buscado no banco em ObterAsync().
public class CarrinhoService(
    IHttpContextAccessor httpContextAccessor,
    IProdutoRepositorio produtoRepositorio,
    ICalculadoraFrete calculadoraFrete,
    IFreteService freteService) : ICarrinhoService
{
    public const int QuantidadeMaxima = 10;
    private const string ChaveSessao = "Carrinho";
    private const string ChaveCep = "CarrinhoCep";

    private record ItemSessao(int VariacaoId, int Quantidade);

    private ISession Sessao => httpContextAccessor.HttpContext!.Session;

    public async Task AdicionarAsync(int variacaoId, int quantidade)
    {
        var variacoes = await produtoRepositorio.ObterVariacoesAsync([variacaoId]);
        if (variacoes.Count == 0)
            throw new InvalidOperationException("Este produto não está disponível.");

        var itens = Ler();
        var atual = itens.FirstOrDefault(i => i.VariacaoId == variacaoId);
        Definir(itens, variacaoId, (atual?.Quantidade ?? 0) + quantidade);
        Gravar(itens);
    }

    public void AlterarQuantidade(int variacaoId, int quantidade)
    {
        var itens = Ler();

        if (quantidade < 1)
            itens.RemoveAll(i => i.VariacaoId == variacaoId);
        else
            Definir(itens, variacaoId, quantidade);

        Gravar(itens);
    }

    public void Remover(int variacaoId) => AlterarQuantidade(variacaoId, 0);

    public void Limpar() => Sessao.Remove(ChaveSessao);

    public int ContarItens() => Ler().Sum(i => i.Quantidade);

    public async Task<Carrinho> ObterAsync()
    {
        var itens = Ler();
        var variacoes = await produtoRepositorio.ObterVariacoesAsync(itens.Select(i => i.VariacaoId));
        var carrinho = new Carrinho();

        // Mantém a ordem em que o cliente adicionou os itens.
        foreach (var item in itens)
        {
            var variacao = variacoes.FirstOrDefault(v => v.Id == item.VariacaoId);
            if (variacao is not null)
                carrinho.Linhas.Add(new LinhaCarrinho(variacao, item.Quantidade));
        }

        // Algum item foi desativado pelo admin: tira da sessão e avisa a tela.
        if (carrinho.Linhas.Count < itens.Count)
        {
            carrinho.ItensForamRemovidos = true;
            Gravar(itens.Where(i => variacoes.Any(v => v.Id == i.VariacaoId)).ToList());
        }

        if (!carrinho.Vazio)
        {
            // Frete grátis pelo valor da compra vale com ou sem CEP; abaixo disso, o valor depende da distância.
            var regra = calculadoraFrete.Calcular(carrinho.Subtotal);
            carrinho.FaltaParaFreteGratis = regra.FaltaParaFreteGratis;

            carrinho.CepFrete = Sessao.GetString(ChaveCep);
            if (carrinho.CepFrete is not null)
                carrinho.Cotacao = await freteService.CotarAsync(carrinho.CepFrete, carrinho.Subtotal);

            carrinho.FreteDefinido = carrinho.Cotacao is not null || regra.Gratis;
            carrinho.Frete = carrinho.Cotacao?.Valor ?? 0;
        }

        return carrinho;
    }

    public void DefinirCep(string? cep)
    {
        var digitos = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digitos.Length == 8)
            Sessao.SetString(ChaveCep, $"{digitos[..5]}-{digitos[5..]}");
        else
            Sessao.Remove(ChaveCep);
    }

    // Atualiza a quantidade no mesmo lugar da lista (ou adiciona no fim), respeitando 1 a 10.
    private static void Definir(List<ItemSessao> itens, int variacaoId, int quantidade)
    {
        var item = new ItemSessao(variacaoId, Math.Clamp(quantidade, 1, QuantidadeMaxima));
        var indice = itens.FindIndex(i => i.VariacaoId == variacaoId);

        if (indice >= 0)
            itens[indice] = item;
        else
            itens.Add(item);
    }

    private List<ItemSessao> Ler()
    {
        var json = Sessao.GetString(ChaveSessao);
        return json is null ? [] : JsonSerializer.Deserialize<List<ItemSessao>>(json) ?? [];
    }

    private void Gravar(List<ItemSessao> itens) =>
        Sessao.SetString(ChaveSessao, JsonSerializer.Serialize(itens));
}
