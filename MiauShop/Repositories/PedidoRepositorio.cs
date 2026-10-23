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
    public Task<Dictionary<StatusPedido, int>> ContarPorStatusAsync() =>
        Conjunto
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Total);

    public async Task<(List<Pedido> Itens, int Total)> ListarParaPainelAsync(
        StatusPedido? status, string? busca, int pagina, int itensPorPagina)
    {
        var consulta = Conjunto.AsQueryable();

        if (status is StatusPedido filtro)
            consulta = consulta.Where(p => p.Status == filtro);

        // Busca por número exato ou por parte do nome/e-mail (ILike = sem diferenciar maiúsculas).
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().TrimStart('#');
            var padrao = $"%{termo}%";
            consulta = int.TryParse(termo, out var numero)
                ? consulta.Where(p => p.Id == numero)
                : consulta.Where(p => EF.Functions.ILike(p.ClienteNome, padrao) || EF.Functions.ILike(p.ClienteEmail, padrao));
        }

        var total = await consulta.CountAsync();

        // Com rastreamento (sem AsNoTracking): o avanço automático pode atualizar estes pedidos.
        var itens = await consulta
            .Include(p => p.Itens)
            .Include(p => p.Historico)
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .AsSplitQuery()
            .ToListAsync();

        return (itens, total);
    }

    public void DefinirVersaoOriginal(Pedido pedido, uint versao) =>
        Contexto.Entry(pedido).Property(p => p.Versao).OriginalValue = versao;

    public async Task<ResumoPedidos> ObterResumoAsync(int quantidadeMaisVendidos, int quantidadeUltimos)
    {
        var porStatus = await ContarPorStatusAsync();

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
