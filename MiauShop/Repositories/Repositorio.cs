using MiauShop.Data;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Repositories;

// CRUD básico escrito uma única vez.
// Categoria e Fornecedor usam esta classe direto; Produto e Pedido herdam dela
// e acrescentam as consultas próprias.
public class Repositorio<T>(AppDbContext contexto) : IRepositorio<T> where T : EntidadeBase
{
    protected readonly AppDbContext Contexto = contexto;
    protected DbSet<T> Conjunto => Contexto.Set<T>();

    public virtual Task<T?> ObterPorIdAsync(int id) =>
        Conjunto.FirstOrDefaultAsync(e => e.Id == id);

    public virtual Task<List<T>> ListarAsync() =>
        Conjunto.AsNoTracking().ToListAsync();

    public void Adicionar(T entidade) => Conjunto.Add(entidade);

    public void Atualizar(T entidade) => Conjunto.Update(entidade);

    public void Remover(T entidade) => Conjunto.Remove(entidade);

    public Task SalvarAsync() => Contexto.SaveChangesAsync();
}
