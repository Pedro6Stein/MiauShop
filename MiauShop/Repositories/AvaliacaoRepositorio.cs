using MiauShop.Data;
using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Repositories;

public class AvaliacaoRepositorio(AppDbContext contexto) : Repositorio<AvaliacaoProduto>(contexto), IAvaliacaoRepositorio
{
    public async Task<ResumoAvaliacoes> ObterResumoAsync(int produtoId, int quantidadeRecentes)
    {
        var aprovadas = Conjunto.AsNoTracking().Where(a => a.ProdutoId == produtoId && a.Aprovada);

        // GROUP BY no banco: só volta uma linha por nota, não todas as avaliações.
        var contagem = await aprovadas
            .GroupBy(a => a.Nota)
            .Select(g => new { Nota = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        var porNota = new int[AvaliacaoProduto.NotaMaxima];
        foreach (var linha in contagem.Where(l => l.Nota is >= AvaliacaoProduto.NotaMinima and <= AvaliacaoProduto.NotaMaxima))
            porNota[linha.Nota - 1] = linha.Quantidade;

        var recentes = await aprovadas
            .OrderByDescending(a => a.CriadoEm)
            .ThenByDescending(a => a.Id)
            .Take(quantidadeRecentes)
            .ToListAsync();

        return new ResumoAvaliacoes(porNota, recentes);
    }

    public Task<List<AvaliacaoProduto>> ListarParaPainelAsync(bool? aprovada)
    {
        var consulta = Conjunto.AsNoTracking().Include(a => a.Produto).AsQueryable();
        if (aprovada is bool situacao)
            consulta = consulta.Where(a => a.Aprovada == situacao);

        return consulta.OrderByDescending(a => a.CriadoEm).ThenByDescending(a => a.Id).Take(200).ToListAsync();
    }

    public Task<int> ContarPendentesAsync() =>
        Conjunto.CountAsync(a => !a.Aprovada);
}
