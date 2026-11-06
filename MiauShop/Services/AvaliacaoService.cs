using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.ViewModels;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class AvaliacaoService(IAvaliacaoRepositorio repositorio) : IAvaliacaoService
{
    // Quantas avaliações aparecem na página do produto.
    public const int QuantidadeNaPagina = 6;

    public Task<ResumoAvaliacoes> ObterResumoAsync(int produtoId) =>
        repositorio.ObterResumoAsync(produtoId, QuantidadeNaPagina);

    // Toda avaliação nova entra pendente: só aparece na loja depois de aprovada no painel.
    public async Task EnviarAsync(int produtoId, NovaAvaliacao dados)
    {
        if (!string.IsNullOrEmpty(dados.Site))
            return; // robô preencheu o campo escondido

        repositorio.Adicionar(AvaliacaoProduto.Nova(produtoId, dados.Nome, dados.Nota, dados.Comentario));
        await repositorio.SalvarAsync();
    }

    public Task<List<AvaliacaoProduto>> ListarParaPainelAsync(bool? aprovada) =>
        repositorio.ListarParaPainelAsync(aprovada);

    public Task<int> ContarPendentesAsync() => repositorio.ContarPendentesAsync();

    public async Task<bool> AprovarAsync(int id)
    {
        var avaliacao = await repositorio.ObterPorIdAsync(id);
        if (avaliacao is null)
            return false;

        avaliacao.Aprovar();
        await repositorio.SalvarAsync();
        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var avaliacao = await repositorio.ObterPorIdAsync(id);
        if (avaliacao is null)
            return false;

        repositorio.Remover(avaliacao);
        await repositorio.SalvarAsync();
        return true;
    }
}
