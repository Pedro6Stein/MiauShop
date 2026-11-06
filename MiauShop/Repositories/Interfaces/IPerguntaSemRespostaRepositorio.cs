using MiauShop.Models.Entities;

namespace MiauShop.Repositories.Interfaces;

public interface IPerguntaSemRespostaRepositorio : IRepositorio<PerguntaSemResposta>
{
    Task<PerguntaSemResposta?> ObterPorTextoAsync(string textoNormalizado);

    // Painel: as mais repetidas primeiro.
    Task<List<PerguntaSemResposta>> ListarMaisFrequentesAsync(int quantidade);
}
