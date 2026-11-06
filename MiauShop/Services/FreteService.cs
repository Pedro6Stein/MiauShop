using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

// Frete = tabela por distância (ConfiguracaoFrete) + regra do frete grátis (ICalculadoraFrete).
// Todo valor sai daqui, no servidor: carrinho, checkout e assistente usam o mesmo cálculo.
public class FreteService(
    IRepositorio<ConfiguracaoFrete> repositorio,
    ILocalizacaoCep localizacao,
    ICalculadoraFrete regraFreteGratis) : IFreteService
{
    // Se nem a origem puder ser localizada (serviços de CEP fora do ar), usa o valor fixo do appsettings.
    private const int DiasTransporteSemDistancia = 3;

    public decimal MinimoParaFreteGratis => regraFreteGratis.MinimoParaFreteGratis;

    public async Task<CotacaoFrete?> CotarAsync(string cep, decimal subtotal)
    {
        var destino = await localizacao.LocalizarAsync(cep);
        if (destino is null)
            return null;

        var configuracao = await ObterConfiguracaoAsync();
        var origem = await localizacao.LocalizarAsync(configuracao.CepOrigem);

        int distancia;
        decimal valorTabela;
        int dias;

        if (origem is null)
        {
            distancia = 0;
            valorTabela = regraFreteGratis.ValorFixo;
            dias = DiasTransporteSemDistancia;
        }
        else
        {
            distancia = CalculoDistancia.EstradaKm(origem.Coordenada, destino.Coordenada);
            valorTabela = configuracao.ValorPara(distancia);
            dias = CalculoDistancia.DiasDeTransporte(distancia);
        }

        var gratis = regraFreteGratis.Calcular(subtotal).Gratis;

        return new CotacaoFrete(
            Valor: gratis ? 0 : valorTabela,
            ValorPelaDistancia: valorTabela,
            DistanciaKm: distancia,
            DiasTransporte: dias,
            Cidade: destino.Cidade,
            Uf: destino.Uf,
            CidadeOrigem: configuracao.CidadeOrigem,
            Aproximado: origem is null || origem.Aproximado || destino.Aproximado);
    }

    // Só existe uma linha de configuração; enquanto ninguém salvar no painel, vale o padrão (Bragança Paulista).
    public async Task<ConfiguracaoFrete> ObterConfiguracaoAsync() =>
        (await repositorio.ListarAsync()).OrderBy(c => c.Id).FirstOrDefault() ?? ConfiguracaoFrete.Padrao();

    public async Task<List<string>> SalvarConfiguracaoAsync(string cepOrigem, decimal valorBase, decimal valorPorKm, decimal valorMaximo)
    {
        var origem = await localizacao.LocalizarAsync(cepOrigem);
        if (origem is null)
            return ["Não encontramos o CEP de origem. Confira os números."];

        var atual = await ObterConfiguracaoAsync();
        var configuracao = atual.Id == 0 ? atual : await repositorio.ObterPorIdAsync(atual.Id) ?? atual;

        configuracao.CepOrigem = origem.Cep;
        configuracao.CidadeOrigem = origem.Cidade;
        configuracao.UfOrigem = origem.Uf;
        configuracao.ValorBase = valorBase;
        configuracao.ValorPorKm = valorPorKm;
        configuracao.ValorMaximo = valorMaximo;

        var erros = configuracao.ValidarRegras().ToList();
        if (erros.Count > 0)
            return erros;

        if (configuracao.Id == 0)
            repositorio.Adicionar(configuracao);

        await repositorio.SalvarAsync();
        return [];
    }
}
