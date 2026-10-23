using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Areas.Admin.ViewModels;

public record BarraStatus(StatusPedido Status, string Nome, int Quantidade, int Percentual);
public record BarraProduto(string Nome, int Quantidade, decimal Receita, int Percentual);

// Tudo o que o dashboard mostra, já calculado no servidor (inclusive o tamanho das barras).
public class DashboardViewModel
{
    public int TotalPedidos { get; init; }
    public int PedidosValidos { get; init; }
    public decimal Faturamento { get; init; }
    public decimal TicketMedio { get; init; }
    public List<BarraStatus> PorStatus { get; init; } = [];
    public List<BarraProduto> MaisVendidos { get; init; } = [];
    public List<Pedido> Ultimos { get; init; } = [];

    public static DashboardViewModel Montar(ResumoPedidos resumo)
    {
        var total = resumo.PorStatus.Values.Sum();
        var validos = total - resumo.PorStatus.GetValueOrDefault(StatusPedido.Cancelado);
        var maiorStatus = Math.Max(resumo.PorStatus.Values.DefaultIfEmpty(0).Max(), 1);
        var maiorProduto = Math.Max(resumo.MaisVendidos.Select(p => p.Quantidade).DefaultIfEmpty(0).Max(), 1);

        return new DashboardViewModel
        {
            TotalPedidos = total,
            PedidosValidos = validos,
            Faturamento = resumo.Faturamento,
            TicketMedio = validos == 0 ? 0 : Math.Round(resumo.Faturamento / validos, 2),

            // Todos os status aparecem, mesmo com zero, na ordem do fluxo do pedido.
            PorStatus = Enum.GetValues<StatusPedido>()
                .Select(status =>
                {
                    var quantidade = resumo.PorStatus.GetValueOrDefault(status);
                    return new BarraStatus(status, status.NomeExibicao(), quantidade, quantidade * 100 / maiorStatus);
                })
                .ToList(),

            MaisVendidos = resumo.MaisVendidos
                .Select(p => new BarraProduto(p.Nome, p.Quantidade, p.Receita, p.Quantidade * 100 / maiorProduto))
                .ToList(),

            Ultimos = resumo.Ultimos
        };
    }
}
