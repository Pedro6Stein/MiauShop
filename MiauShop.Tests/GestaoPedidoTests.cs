using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Models.ValueObjects;
using MiauShop.Services;
using Microsoft.Extensions.Options;

namespace MiauShop.Tests;

public class GestaoPedidoTests
{
    private static Pedido NovoPedido(DateTime? criadoEm = null)
    {
        var pedido = Pedido.Criar("Ana", "ana@email.com", "11999999999", new Endereco(), FormaPagamento.Pix);
        pedido.CriadoEm = criadoEm ?? DateTime.UtcNow;
        return pedido;
    }

    private static AvancoAutomatico Avanco(bool habilitado) =>
        new(Options.Create(new OpcoesAvancoAutomatico
        {
            Habilitado = habilitado,
            MinutosAtePagamento = 1,
            MinutosAteEnvio = 5,
            MinutosAteEntrega = 15
        }));

    [Fact]
    public void CancelarSemMotivo_ERecusado()
    {
        var pedido = NovoPedido();

        var erro = Assert.Throws<InvalidOperationException>(() =>
            pedido.AlterarStatus(StatusPedido.Cancelado, OrigemStatus.Admin, "   "));

        Assert.Equal("Informe o motivo do cancelamento.", erro.Message);
        Assert.Equal(StatusPedido.Recebido, pedido.Status);
    }

    [Fact]
    public void ProximosStatus_SeguemAsTransicoesPermitidas()
    {
        var pedido = NovoPedido();
        Assert.Equal([StatusPedido.PagamentoAprovado, StatusPedido.Cancelado], pedido.ProximosStatusPossiveis());

        pedido.AlterarStatus(StatusPedido.PagamentoAprovado, OrigemStatus.Admin);
        pedido.AlterarStatus(StatusPedido.Enviado, OrigemStatus.Admin);
        Assert.Equal([StatusPedido.Entregue], pedido.ProximosStatusPossiveis());

        pedido.AlterarStatus(StatusPedido.Entregue, OrigemStatus.Admin);
        Assert.Empty(pedido.ProximosStatusPossiveis());
    }

    [Fact]
    public void AvancoAutomaticoDesligado_NaoMudaNada()
    {
        var pedido = NovoPedido(DateTime.UtcNow.AddHours(-2));

        Assert.False(Avanco(habilitado: false).Aplicar(pedido, DateTime.UtcNow));
        Assert.Equal(StatusPedido.Recebido, pedido.Status);
    }

    [Fact]
    public void AvancoAutomaticoLigado_AvancaConformeOTempo_ERegistraOHorarioDeCadaEtapa()
    {
        var criado = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        var pedido = NovoPedido(criado);

        Assert.True(Avanco(habilitado: true).Aplicar(pedido, criado.AddMinutes(6)));

        Assert.Equal(StatusPedido.Enviado, pedido.Status);
        Assert.Equal(criado.AddMinutes(5), pedido.Historico.Single(h => h.Status == StatusPedido.Enviado).CriadoEm);
    }

    [Fact]
    public void AvancoAutomatico_NaoMexeEmPedidoCancelado()
    {
        var pedido = NovoPedido(DateTime.UtcNow.AddHours(-2));
        pedido.AlterarStatus(StatusPedido.Cancelado, OrigemStatus.Admin, "Sem estoque no fornecedor.");

        Assert.False(Avanco(habilitado: true).Aplicar(pedido, DateTime.UtcNow));
        Assert.Equal(StatusPedido.Cancelado, pedido.Status);
    }
}
