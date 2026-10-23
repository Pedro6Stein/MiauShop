using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Models;
using MiauShop.Models.Enums;

namespace MiauShop.Tests;

public class DashboardTests
{
    [Fact]
    public void Cancelados_NaoEntramNoTicketMedio_EBarrasSaoProporcionais()
    {
        var resumo = new ResumoPedidos(
            new() { [StatusPedido.Recebido] = 2, [StatusPedido.Entregue] = 1, [StatusPedido.Cancelado] = 1 },
            Faturamento: 300m,
            MaisVendidos: [new(1, "Moletom", 4, 220m), new(2, "Bandana", 2, 49.80m)],
            Ultimos: []);

        var painel = DashboardViewModel.Montar(resumo);

        Assert.Equal(4, painel.TotalPedidos);
        Assert.Equal(3, painel.PedidosValidos);
        Assert.Equal(100m, painel.TicketMedio);
        Assert.Equal(100, painel.PorStatus.Single(s => s.Status == StatusPedido.Recebido).Percentual);
        Assert.Equal(50, painel.PorStatus.Single(s => s.Status == StatusPedido.Entregue).Percentual);
        Assert.Equal(0, painel.PorStatus.Single(s => s.Status == StatusPedido.Enviado).Quantidade);
        Assert.Equal(50, painel.MaisVendidos[1].Percentual);
    }
}
