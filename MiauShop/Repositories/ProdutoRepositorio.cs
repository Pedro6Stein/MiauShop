using MiauShop.Data;
using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Repositories;

public class ProdutoRepositorio(AppDbContext contexto) : Repositorio<Produto>(contexto), IProdutoRepositorio
{
    // Ponto de partida das consultas da loja: só produtos ativos de categorias ativas,
    // já com categoria e imagens (o card mostra as duas).
    private IQueryable<Produto> ProdutosDaLoja() =>
        Conjunto.AsNoTracking()
            .Where(p => p.Ativo && p.Categoria.Ativa)
            .Include(p => p.Categoria)
            .Include(p => p.Imagens)
            .Include(p => p.Variacoes.Where(v => v.Ativa));

    public Task<List<Produto>> ListarDestaquesAsync(int quantidade) =>
        ProdutosDaLoja()
            .Where(p => p.Destaque)
            .OrderByDescending(p => p.CriadoEm)
            .Take(quantidade)
            .ToListAsync();

    public async Task<(List<Produto> Itens, int Total)> BuscarAsync(FiltroProdutos filtro, int itensPorPagina)
    {
        var consulta = ProdutosDaLoja();

        // Filtrar por "Gato" também traz os produtos marcados como "Ambos".
        if (filtro.Animal is Animal animal)
            consulta = consulta.Where(p => p.Animal == animal || p.Animal == Animal.Ambos);

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
            consulta = consulta.Where(p => p.Categoria.Slug == filtro.Categoria);

        // ILike = "contém", sem diferenciar maiúsculas de minúsculas (recurso do PostgreSQL).
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = $"%{filtro.Busca.Trim()}%";
            consulta = consulta.Where(p => EF.Functions.ILike(p.Nome, termo) || EF.Functions.ILike(p.DescricaoCurta, termo));
        }

        if (filtro.Promocao)
            consulta = consulta.Where(p => p.PrecoPromocional < p.Preco);

        // Preço atual = promocional (se for menor) ou normal. A mesma regra de Produto.PrecoAtual(),
        // escrita aqui de um jeito que o EF consegue traduzir para SQL.
        if (filtro.PrecoMin is int minimo)
            consulta = consulta.Where(p => (p.PrecoPromocional < p.Preco ? p.PrecoPromocional!.Value : p.Preco) >= minimo);

        if (filtro.PrecoMax is int maximo)
            consulta = consulta.Where(p => (p.PrecoPromocional < p.Preco ? p.PrecoPromocional!.Value : p.Preco) <= maximo);

        var total = await consulta.CountAsync();

        var ordenada = filtro.Ordem switch
        {
            OrdenacaoProdutos.MenorPreco => consulta.OrderBy(p => p.PrecoPromocional < p.Preco ? p.PrecoPromocional!.Value : p.Preco),
            OrdenacaoProdutos.MaiorPreco => consulta.OrderByDescending(p => p.PrecoPromocional < p.Preco ? p.PrecoPromocional!.Value : p.Preco),
            OrdenacaoProdutos.MaiorDesconto => consulta.OrderByDescending(p => p.PrecoPromocional < p.Preco ? (p.Preco - p.PrecoPromocional!.Value) / p.Preco : 0),
            OrdenacaoProdutos.Novidades => consulta.OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id),
            OrdenacaoProdutos.NomeAZ => consulta.OrderBy(p => p.Nome),
            _ => consulta.OrderByDescending(p => p.Destaque)
        };

        var itens = await ordenada
            .ThenBy(p => p.Nome) // desempate estável: a paginação não repete nem pula produtos
            .Skip((filtro.Pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        return (itens, total);
    }

    public Task<Produto?> ObterPorSlugAsync(string slug) =>
        ProdutosDaLoja()
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public Task<List<ProdutoVariacao>> ObterVariacoesAsync(IEnumerable<int> variacaoIds) =>
        Contexto.ProdutoVariacoes.AsNoTracking()
            .Where(v => variacaoIds.Contains(v.Id) && v.Ativa && v.Produto.Ativo)
            .Include(v => v.Produto).ThenInclude(p => p.Imagens)
            .ToListAsync();
}
