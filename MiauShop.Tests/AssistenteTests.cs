using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Models.ValueObjects;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services;
using MiauShop.Services.Interfaces;

namespace MiauShop.Tests;

public class AssistenteTests
{
    private static readonly List<PerguntaFrequente> Perguntas =
    [
        new() { Id = 1, Ordem = 1, Pergunta = "O frete é grátis?", Resposta = "Frete grátis acima de R$ 150.", PalavrasChave = "frete gratis, gratis" },
        new() { Id = 2, Ordem = 2, Pergunta = "Posso trocar ou devolver?", Resposta = "Você tem 7 dias para desistir.", PalavrasChave = "troca, trocar, devolver, devolucao" },
        new() { Id = 3, Ordem = 3, Pergunta = "Pergunta desligada", Resposta = "Não deve aparecer.", PalavrasChave = "desligada", Ativa = false }
    ];

    // ---------- Buscador (regras sem IA) ----------

    [Fact]
    public void Normalizar_TiraAcentosMaiusculasEPontuacao()
    {
        Assert.Equal("racao p gatos", BuscadorDeRespostas.Normalizar("  Ração p/ GATOS!! "));
    }

    [Theory]
    [InlineData("Quero DEVOLUÇÃO desse produto", 2)]
    [InlineData("dá pra trocar o tamanho?", 2)]
    [InlineData("a entrega é grátis?", 1)]
    public void MelhorResposta_AchaPelaPalavraChaveSemAcento(string mensagem, int idEsperado)
    {
        Assert.Equal(idEsperado, BuscadorDeRespostas.MelhorResposta(mensagem, Perguntas)?.Id);
    }

    [Fact]
    public void MelhorResposta_AssuntoSemRelacao_DevolveNull()
    {
        Assert.Null(BuscadorDeRespostas.MelhorResposta("qual a cor do céu?", Perguntas));
    }

    [Fact]
    public void PalavraChave_CasaSoNoComecoDaPalavra()
    {
        // "troca" não pode casar com "destroca"
        Assert.Null(BuscadorDeRespostas.MelhorResposta("destroca", Perguntas));
    }

    [Theory]
    [InlineData("quanto fica pro 01310-100?", "01310100")]
    [InlineData("meu cep é 12900002", "12900002")]
    [InlineData("pedido 123", null)]
    public void EncontrarCep_NoMeioDaFrase(string texto, string? esperado)
    {
        Assert.Equal(esperado, BuscadorDeRespostas.EncontrarCep(texto));
    }

    [Theory]
    [InlineData("quero rastrear minha compra", EtapasAssistente.PedidoNumero)]
    [InlineData("quero falar com um atendente", EtapasAssistente.Atendente)]
    [InlineData("calcular frete", EtapasAssistente.Frete)]
    [InlineData("tem caminha?", null)]
    public void IdentificarAcao(string texto, string? etapa)
    {
        Assert.Equal(etapa, BuscadorDeRespostas.IdentificarAcao(texto));
    }

    [Theory]
    [InlineData("contato: ana@email.com", true)]
    [InlineData("meu telefone 11 99999-8888", true)]
    [InlineData("vocês vendem coleira?", false)]
    public void PareceDadoPessoal(string texto, bool esperado)
    {
        Assert.Equal(esperado, BuscadorDeRespostas.PareceDadoPessoal(texto));
    }

    // ---------- Conversa (roteiro do assistente) ----------

    [Fact]
    public async Task Inicio_MostraSaudacaoEMenuNumerado()
    {
        var resposta = await Novo().ResponderAsync(EtapasAssistente.Inicio, null, null);

        Assert.Contains("Miau", resposta.Mensagens[0]);
        Assert.Equal(["1", "2", "3", "4", "5"], resposta.Opcoes.Select(o => o.Valor));
        Assert.Equal(EtapasAssistente.Menu, resposta.Etapa);
    }

    [Fact]
    public async Task OpcaoUm_ListaSoAsDuvidasAtivas_ENumeroMostraAResposta()
    {
        var assistente = Novo();

        var lista = await assistente.ResponderAsync(EtapasAssistente.Menu, "1", null);
        Assert.Equal(EtapasAssistente.Duvidas, lista.Etapa);
        Assert.DoesNotContain(lista.Opcoes, o => o.Texto == "Pergunta desligada");

        var resposta = await assistente.ResponderAsync(lista.Etapa, "2", null);
        Assert.Contains("7 dias", resposta.Mensagens[0]);
    }

    [Fact]
    public async Task Frete_UsaOServicoDeFreteEGuardaOCepNoCarrinho()
    {
        var carrinho = new CarrinhoFalso();
        var assistente = Novo(carrinho);

        var pergunta = await assistente.ResponderAsync(EtapasAssistente.Menu, "3", null);
        Assert.Equal(EtapasAssistente.Frete, pergunta.Etapa);

        var resposta = await assistente.ResponderAsync(pergunta.Etapa, "01310-100", null);
        Assert.Contains("São Paulo/SP", resposta.Mensagens[0]);
        Assert.Contains("15,60", resposta.Mensagens[0]); // valor vem do serviço de frete, não do assistente
        Assert.Equal("01310-100", carrinho.CepDefinido);
    }

    [Fact]
    public async Task Frete_CepInexistente_PedeDeNovo()
    {
        var resposta = await Novo().ResponderAsync(EtapasAssistente.Frete, "99999-999", null);

        Assert.Equal(EtapasAssistente.Frete, resposta.Etapa);
        Assert.Contains("Não encontrei", resposta.Mensagens[0]);
    }

    [Fact]
    public async Task Pedido_PedeNumeroDepoisEmail_EMostraOStatus()
    {
        var assistente = Novo();

        var email = await assistente.ResponderAsync(EtapasAssistente.PedidoNumero, "#7", null);
        Assert.Equal(EtapasAssistente.PedidoEmail, email.Etapa);
        Assert.Equal("7", email.Contexto);

        var status = await assistente.ResponderAsync(email.Etapa, "ana@teste.com", email.Contexto);
        Assert.Contains("Pedido #7: Recebido", status.Mensagens[0]);
        Assert.Contains(status.Links, l => l.Url == "/Pedido/Acompanhar?numero=7");
    }

    [Fact]
    public async Task Pedido_EmailErrado_NaoRevelaNada()
    {
        var resposta = await Novo().ResponderAsync(EtapasAssistente.PedidoEmail, "outro@teste.com", "7");

        Assert.Contains("Não encontrei", resposta.Mensagens[0]);
    }

    [Fact]
    public async Task TextoLivre_PalavraChave_RespondeComOCadastro()
    {
        var resposta = await Novo().ResponderAsync(EtapasAssistente.Menu, "como faço para devolver?", null);

        Assert.Contains("7 dias", resposta.Mensagens[0]);
    }

    [Fact]
    public async Task TextoLivre_NomeDeProduto_MostraProdutosComPrecoDoServidor()
    {
        var resposta = await Novo().ResponderAsync(EtapasAssistente.Menu, "vocês têm caminha para gato?", null);

        var produto = Assert.Single(resposta.Produtos);
        Assert.Equal("Caminha Nuvem", produto.Nome);
        Assert.Equal(119.90m, produto.Preco);
    }

    [Fact]
    public async Task TextoLivre_SemResposta_FicaAnotadoUmaVezSo()
    {
        var anotadas = new SemRespostaFalso();
        var assistente = Novo(semResposta: anotadas);

        await assistente.ResponderAsync(EtapasAssistente.Menu, "Vocês abrem no domingo?", null);
        var resposta = await assistente.ResponderAsync(EtapasAssistente.Menu, "voces abrem no DOMINGO", null);

        Assert.Contains("Anotei", resposta.Mensagens[0]);
        var anotada = Assert.Single(anotadas.Itens);
        Assert.Equal(2, anotada.Vezes);
    }

    [Fact]
    public async Task TextoLivre_ComEmail_NaoEhAnotado()
    {
        var anotadas = new SemRespostaFalso();

        await Novo(semResposta: anotadas).ResponderAsync(EtapasAssistente.Menu, "me responde em ana@teste.com", null);

        Assert.Empty(anotadas.Itens);
    }

    [Fact]
    public async Task Cumprimento_VoltaParaASaudacao()
    {
        var resposta = await Novo().ResponderAsync(EtapasAssistente.Menu, "Oi!", null);

        Assert.Contains("Miau", resposta.Mensagens[0]);
    }

    [Fact]
    public async Task Zero_VoltaAoMenuDeQualquerEtapa()
    {
        var resposta = await Novo().ResponderAsync(EtapasAssistente.PedidoEmail, "0", "7");

        Assert.Equal(EtapasAssistente.Menu, resposta.Etapa);
        Assert.Null(resposta.Contexto);
    }

    // ---------- Montagem com dublês (sem banco e sem internet) ----------

    private static AssistenteService Novo(CarrinhoFalso? carrinho = null, SemRespostaFalso? semResposta = null) =>
        new(new PerguntasFalsas(), semResposta ?? new SemRespostaFalso(), new CatalogoFalso(),
            new FreteFalso(), carrinho ?? new CarrinhoFalso(), new PedidosFalsos());

    private class PerguntasFalsas : IRepositorio<PerguntaFrequente>
    {
        public Task<PerguntaFrequente?> ObterPorIdAsync(int id) => Task.FromResult(Perguntas.FirstOrDefault(p => p.Id == id));
        public Task<List<PerguntaFrequente>> ListarAsync() => Task.FromResult(Perguntas.ToList());
        public void Adicionar(PerguntaFrequente entidade) { }
        public void Atualizar(PerguntaFrequente entidade) { }
        public void Remover(PerguntaFrequente entidade) { }
        public Task SalvarAsync() => Task.CompletedTask;
    }

    private class SemRespostaFalso : IPerguntaSemRespostaRepositorio
    {
        public List<PerguntaSemResposta> Itens { get; } = [];
        public Task<PerguntaSemResposta?> ObterPorTextoAsync(string textoNormalizado) =>
            Task.FromResult(Itens.FirstOrDefault(i => i.TextoNormalizado == textoNormalizado));
        public Task<List<PerguntaSemResposta>> ListarMaisFrequentesAsync(int quantidade) => Task.FromResult(Itens.ToList());
        public Task<PerguntaSemResposta?> ObterPorIdAsync(int id) => Task.FromResult(Itens.FirstOrDefault(i => i.Id == id));
        public Task<List<PerguntaSemResposta>> ListarAsync() => Task.FromResult(Itens.ToList());
        public void Adicionar(PerguntaSemResposta entidade) => Itens.Add(entidade);
        public void Atualizar(PerguntaSemResposta entidade) { }
        public void Remover(PerguntaSemResposta entidade) => Itens.Remove(entidade);
        public Task SalvarAsync() => Task.CompletedTask;
    }

    private class CatalogoFalso : ICatalogoService
    {
        private static readonly List<Produto> Produtos =
        [
            new() { Id = 1, Nome = "Caminha Nuvem", Slug = "caminha-nuvem", Preco = 149.90m, PrecoPromocional = 119.90m },
            new() { Id = 2, Nome = "Bandana Xadrez", Slug = "bandana-xadrez", Preco = 24.90m }
        ];

        public Task<(List<Produto> Itens, int Total)> BuscarAsync(FiltroProdutos filtro)
        {
            var itens = Produtos.Where(p => p.Nome.Contains(filtro.Busca ?? "", StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult((itens, itens.Count));
        }

        public Task<List<Produto>> ListarDestaquesAsync() => Task.FromResult(Produtos.ToList());
        public Task<Produto?> ObterProdutoAsync(string slug) => Task.FromResult(Produtos.FirstOrDefault(p => p.Slug == slug));
        public Task<List<Produto>> ListarRelacionadosAsync(Produto produto) => Task.FromResult(new List<Produto>());
        public Task<List<Categoria>> ListarCategoriasAsync() => Task.FromResult(new List<Categoria>());
    }

    private class FreteFalso : IFreteService
    {
        public decimal MinimoParaFreteGratis => 150m;

        public Task<CotacaoFrete?> CotarAsync(string cep, decimal subtotal) =>
            Task.FromResult(new string(cep.Where(char.IsDigit).ToArray()) == "01310100"
                ? new CotacaoFrete(15.60m, 15.60m, 90, 1, "São Paulo", "SP", "Bragança Paulista", false)
                : null);

        public Task<ConfiguracaoFrete> ObterConfiguracaoAsync() => Task.FromResult(ConfiguracaoFrete.Padrao());
        public Task<List<string>> SalvarConfiguracaoAsync(string cepOrigem, decimal valorBase, decimal valorPorKm, decimal valorMaximo) =>
            Task.FromResult(new List<string>());
    }

    private class CarrinhoFalso : ICarrinhoService
    {
        public string? CepDefinido { get; private set; }

        public Task<Carrinho> ObterAsync() => Task.FromResult(new Carrinho());
        public void DefinirCep(string? cep)
        {
            var digitos = new string((cep ?? "").Where(char.IsDigit).ToArray());
            CepDefinido = digitos.Length == 8 ? $"{digitos[..5]}-{digitos[5..]}" : null;
        }

        public Task AdicionarAsync(int variacaoId, int quantidade) => Task.CompletedTask;
        public void AlterarQuantidade(int variacaoId, int quantidade) { }
        public void Remover(int variacaoId) { }
        public void Limpar() { }
        public int ContarItens() => 0;
    }

    private class PedidosFalsos : IPedidoService
    {
        public Task<Pedido?> ObterAsync(int pedidoId) => Task.FromResult<Pedido?>(null);

        public Task<Pedido?> ObterParaAcompanhamentoAsync(int pedidoId, string email)
        {
            if (pedidoId != 7 || email.Trim().ToLowerInvariant() != "ana@teste.com")
                return Task.FromResult<Pedido?>(null);

            var pedido = Pedido.Criar("Ana", "ana@teste.com", "11999999999", new Endereco(), FormaPagamento.Pix);
            pedido.Id = 7;
            return Task.FromResult<Pedido?>(pedido);
        }
    }
}
