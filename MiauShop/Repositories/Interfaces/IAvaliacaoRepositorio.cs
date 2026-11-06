using MiauShop.Models;
using MiauShop.Models.Entities;

namespace MiauShop.Repositories.Interfaces;

public interface IAvaliacaoRepositorio : IRepositorio<AvaliacaoProduto>
{
    // Loja: contagem por nota e as mais recentes, só das aprovadas.
    Task<ResumoAvaliacoes> ObterResumoAsync(int produtoId, int quantidadeRecentes);

    // Painel: pendentes (aprovada = false), aprovadas (true) ou todas (null), com o produto.
    Task<List<AvaliacaoProduto>> ListarParaPainelAsync(bool? aprovada);

    Task<int> ContarPendentesAsync();
}
