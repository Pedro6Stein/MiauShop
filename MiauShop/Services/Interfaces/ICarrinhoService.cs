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

    // CEP usado no cálculo do frete (fica na sessão). CEP inválido ou vazio apaga o atual.
    void DefinirCep(string? cep);
}
