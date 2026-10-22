using MiauShop.Models;

namespace MiauShop.Services.Interfaces;

public interface ICarrinhoService
{
    Task AdicionarAsync(int variacaoId, int quantidade);
    void AlterarQuantidade(int variacaoId, int quantidade);
    void Remover(int variacaoId);
    void Limpar();
    int ContarItens();
    Task<Carrinho> ObterAsync();
}
