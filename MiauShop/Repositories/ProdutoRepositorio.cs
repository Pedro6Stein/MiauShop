using MiauShop.Data;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Repositories;

public class ProdutoRepositorio(AppDbContext contexto) : Repositorio<Produto>(contexto), IProdutoRepositorio
{
    // Ponto de partida das consultas da loja: só produtos ativos de categorias ativas,
    // já com as imagens (o card precisa da imagem principal).
    private IQueryable<Produto> ProdutosDaLoja() =>
        Conjunto.AsNoTracking()
            .Where(p => p.Ativo && p.Categoria.Ativa)
            .Include(p => p.Imagens)
            .Include(p => p.Variacoes.Where(v => v.Ativa));

    public Task<List<Produto>> ListarDestaquesAsync(int quantidade) =>
        ProdutosDaLoja()
            .Where(p => p.Destaque)
            .OrderByDescending(p => p.CriadoEm)
            .Take(quantidade)
            .ToListAsync();

    public async Task<(List<Produto> Itens, int Total)> BuscarAsync(
        Animal? animal, string? categoriaSlug, string? texto, int pagina, int itensPorPagina)
    {
        var consulta = ProdutosDaLoja();

        // Filtrar por "Gato" também traz os produtos marcados como "Ambos".
        if (animal is not null)
            consulta = consulta.Where(p => p.Animal == animal || p.Animal == Animal.Ambos);

        if (!string.IsNullOrWhiteSpace(categoriaSlug))
            consulta = consulta.Where(p => p.Categoria.Slug == categoriaSlug);

        // ILike = "contém", sem diferenciar maiúsculas de minúsculas (recurso do PostgreSQL).
        if (!string.IsNullOrWhiteSpace(texto))
            consulta = consulta.Where(p => EF.Functions.ILike(p.Nome, $"%{texto.Trim()}%"));

        var total = await consulta.CountAsync();

        var itens = await consulta
            .OrderByDescending(p => p.Destaque)
            .ThenBy(p => p.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        return (itens, total);
    }

    public Task<Produto?> ObterPorSlugAsync(string slug) =>
        ProdutosDaLoja()
            .Include(p => p.Categoria)
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public Task<List<ProdutoVariacao>> ObterVariacoesAsync(IEnumerable<int> variacaoIds) =>
        Contexto.ProdutoVariacoes.AsNoTracking()
            .Where(v => variacaoIds.Contains(v.Id) && v.Ativa && v.Produto.Ativo)
            .Include(v => v.Produto).ThenInclude(p => p.Imagens)
            .ToListAsync();
}
