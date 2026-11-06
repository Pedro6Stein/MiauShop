using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MiauShop.Tests;

public class FreteDistanciaTests
{
    private static readonly Coordenada Braganca = new(-22.954923, -46.54);        // CEP 12900-002
    private static readonly Coordenada AvenidaPaulista = new(-23.5475, -46.63611); // CEP 01310-100
    private static readonly Coordenada Manaus = new(-3.119, -60.022);

    [Fact]
    public void Distancia_BragancaAteSaoPaulo_FicaPertoDoReal()
    {
        var linhaReta = CalculoDistancia.LinhaRetaKm(Braganca, AvenidaPaulista);

        Assert.InRange(linhaReta, 60, 72);                                        // ~66 km em linha reta
        Assert.InRange(CalculoDistancia.EstradaKm(Braganca, AvenidaPaulista), 78, 94); // ~86 km pela estrada
    }

    [Fact]
    public void Distancia_MesmoPonto_ContaPeloMenosUmKm()
    {
        Assert.Equal(1, CalculoDistancia.EstradaKm(Braganca, Braganca));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(499, 1)]
    [InlineData(500, 2)]
    [InlineData(3500, 8)]
    public void DiasDeTransporte_UmDiaACada500Km(int km, int dias)
    {
        Assert.Equal(dias, CalculoDistancia.DiasDeTransporte(km));
    }

    [Fact]
    public void Tabela_SomaBaseMaisKmERespeitaOMaximo()
    {
        var tabela = ConfiguracaoFrete.Padrao(); // base 12,00 · 0,04/km · máximo 59,90

        Assert.Equal(15.60m, tabela.ValorPara(90));
        Assert.Equal(59.90m, tabela.ValorPara(4000));
    }

    [Fact]
    public void Tabela_ValoresInvalidos_SaoRecusados()
    {
        var tabela = new ConfiguracaoFrete { ValorBase = 20, ValorMaximo = 10, ValorPorKm = -1 };

        var erros = tabela.ValidarRegras().ToList();

        Assert.Equal(2, erros.Count);
    }

    [Fact]
    public void CapitalDoEstado_EhOPlanoB()
    {
        Assert.Equal(new Coordenada(-23.550, -46.633), Coordenada.CapitalDe("sp"));
        Assert.Null(Coordenada.CapitalDe("XX"));
        Assert.Null(Coordenada.CapitalDe(null));
    }

    [Fact]
    public async Task Cotacao_AbaixoDoMinimo_CobraPelaDistancia()
    {
        var servico = Servico(new()
        {
            ["12900-002"] = new("12900-002", "Bragança Paulista", "SP", Braganca, false),
            ["01310-100"] = new("01310-100", "São Paulo", "SP", AvenidaPaulista, false)
        });

        var cotacao = await servico.CotarAsync("01310100", subtotal: 80);

        Assert.NotNull(cotacao);
        Assert.False(cotacao.Gratis);
        Assert.Equal(ConfiguracaoFrete.Padrao().ValorPara(cotacao.DistanciaKm), cotacao.Valor);
        Assert.Equal("São Paulo", cotacao.Cidade);
        Assert.Equal("Bragança Paulista", cotacao.CidadeOrigem);
        Assert.False(cotacao.Aproximado);
    }

    [Fact]
    public async Task Cotacao_AcimaDoMinimo_EhGratisMasGuardaOValorDaTabela()
    {
        var servico = Servico(new()
        {
            ["12900-002"] = new("12900-002", "Bragança Paulista", "SP", Braganca, false),
            ["69005-000"] = new("69005-000", "Manaus", "AM", Manaus, false)
        });

        var cotacao = await servico.CotarAsync("69005-000", subtotal: 200);

        Assert.NotNull(cotacao);
        Assert.True(cotacao.Gratis);
        Assert.Equal(59.90m, cotacao.ValorPelaDistancia); // longe: bate no máximo
        Assert.True(cotacao.DiasTransporte >= 7);
    }

    [Fact]
    public async Task Cotacao_CepInexistente_DevolveNull()
    {
        var servico = Servico(new() { ["12900-002"] = new("12900-002", "Bragança Paulista", "SP", Braganca, false) });

        Assert.Null(await servico.CotarAsync("99999-999", 50));
    }

    [Fact]
    public async Task Cotacao_SemOrigem_UsaOValorFixoDaConfiguracao()
    {
        var servico = Servico(new() { ["01310-100"] = new("01310-100", "São Paulo", "SP", AvenidaPaulista, false) });

        var cotacao = await servico.CotarAsync("01310-100", 50);

        Assert.NotNull(cotacao);
        Assert.Equal(19.90m, cotacao.Valor);
        Assert.True(cotacao.Aproximado);
    }

    private static FreteService Servico(Dictionary<string, LocalCep> ceps) =>
        new(new RepositorioVazio(), new LocalizacaoFalsa(ceps),
            new FreteFixoComGratis(Options.Create(new OpcoesFrete { ValorFixo = 19.90m, MinimoParaFreteGratis = 150m })));

    private class LocalizacaoFalsa(Dictionary<string, LocalCep> ceps) : ILocalizacaoCep
    {
        public Task<LocalCep?> LocalizarAsync(string cep)
        {
            var digitos = new string(cep.Where(char.IsDigit).ToArray());
            var chave = digitos.Length == 8 ? $"{digitos[..5]}-{digitos[5..]}" : cep;
            return Task.FromResult(ceps.GetValueOrDefault(chave));
        }
    }

    // Nenhuma tabela salva no banco: vale a configuração padrão (Bragança Paulista).
    private class RepositorioVazio : IRepositorio<ConfiguracaoFrete>
    {
        public Task<ConfiguracaoFrete?> ObterPorIdAsync(int id) => Task.FromResult<ConfiguracaoFrete?>(null);
        public Task<List<ConfiguracaoFrete>> ListarAsync() => Task.FromResult(new List<ConfiguracaoFrete>());
        public void Adicionar(ConfiguracaoFrete entidade) { }
        public void Atualizar(ConfiguracaoFrete entidade) { }
        public void Remover(ConfiguracaoFrete entidade) { }
        public Task SalvarAsync() => Task.CompletedTask;
    }
}
