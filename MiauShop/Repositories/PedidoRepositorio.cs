using MiauShop.Data;
using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Repositories;

public class PedidoRepositorio(AppDbContext contexto) : Repositorio<Pedido>(contexto), IPedidoRepositorio
{
    public Task<Pedido?> ObterCompletoAsync(int id) =>
        Conjunto
            .Include(p => p.Itens)
            .Include(p => p.Historico)
            .FirstOrDefaultAsync(p => p.Id == id);

    // Tudo calculado pelo banco (GROUP BY / SUM). Pedidos cancelados não contam como venda.
    public async Task<ResumoPedidos> ObterResumoAsync(int quantidadeMaisVendidos, int quantidadeUltimos)
    {
        var porStatus = await Conjunto
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Total);

        var faturamento = await Conjunto
            .Where(p => p.Status != StatusPedido.Cancelado)
            .SumAsync(p => (decimal?)p.Total) ?? 0;

        // Agrupa e ordena no banco (tipo anônimo, que o EF traduz para SQL) e só depois monta o record.
        var maisVendidos = (await Contexto.ItensPedido
                .Where(i => i.Pedido.Status != StatusPedido.Cancelado)
                .GroupBy(i => new { i.ProdutoVariacao.ProdutoId, i.ProdutoVariacao.Produto.Nome })
                .Select(g => new
                {
                    g.Key.ProdutoId,
                    g.Key.Nome,
                    Quantidade = g.Sum(i => i.Quantidade),
                    Receita = g.Sum(i => i.PrecoUnitario * i.Quantidade)
                })
                .OrderByDescending(x => x.Quantidade)
                .Take(quantidadeMaisVendidos)
                .ToListAsync())
            .Select(x => new ProdutoMaisVendido(x.ProdutoId, x.Nome, x.Quantidade, x.Receita))
            .ToList();

        var ultimos = await Conjunto.AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .Take(quantidadeUltimos)
            .ToListAsync();

        return new ResumoPedidos(porStatus, faturamento, maisVendidos, ultimos);
    }
}
