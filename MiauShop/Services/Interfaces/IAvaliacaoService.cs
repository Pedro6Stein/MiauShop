using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.ViewModels;

namespace MiauShop.Services.Interfaces;

public interface IAvaliacaoService
{
    // Loja
    Task<ResumoAvaliacoes> ObterResumoAsync(int produtoId);
    Task EnviarAsync(int produtoId, NovaAvaliacao dados);

    // Painel
    Task<List<AvaliacaoProduto>> ListarParaPainelAsync(bool? aprovada);
    Task<int> ContarPendentesAsync();
    Task<bool> AprovarAsync(int id);
    Task<bool> RemoverAsync(int id);
}
