using MiauShop.Models.Entities;
using MiauShop.Models.ViewModels;

namespace MiauShop.Services.Interfaces;

public interface IAssistenteService
{
    // Loja: responde a mensagem do cliente de acordo com a etapa da conversa.
    Task<RespostaAssistente> ResponderAsync(string? etapa, string? texto, string? contexto);

    // Painel
    Task<List<PerguntaFrequente>> ListarPerguntasAsync();
    Task<PerguntaFrequente?> ObterPerguntaAsync(int id);
    Task SalvarPerguntaAsync(int id, string pergunta, string resposta, string? palavrasChave, int ordem, bool ativa);
    Task<bool> ExcluirPerguntaAsync(int id);

    Task<List<PerguntaSemResposta>> ListarSemRespostaAsync();
    Task<PerguntaSemResposta?> ObterSemRespostaAsync(int id);
    Task<bool> ExcluirSemRespostaAsync(int id);
}
