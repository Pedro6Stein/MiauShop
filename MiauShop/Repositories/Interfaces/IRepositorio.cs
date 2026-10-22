using MiauShop.Models.Entities;

namespace MiauShop.Repositories.Interfaces;

// Contrato comum a todos os repositórios.
// "where T : EntidadeBase" garante que só entidades da loja usam esta interface.
public interface IRepositorio<T> where T : EntidadeBase
{
    Task<T?> ObterPorIdAsync(int id);
    Task<List<T>> ListarAsync();
    void Adicionar(T entidade);
    void Atualizar(T entidade);
    Task SalvarAsync();
}
