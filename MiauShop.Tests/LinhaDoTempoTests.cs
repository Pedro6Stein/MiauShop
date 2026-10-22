using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Models.ValueObjects;
using MiauShop.Services;

namespace MiauShop.Tests;

public class LinhaDoTempoTests
{
    private static Pedido NovoPedido() =>
        Pedido.Criar("Ana", "ana@email.com", "11999999999", new Endereco(), FormaPagamento.Pix);

    [Fact]
    public void PedidoRecebido_PrimeiraEtapaAtualEResto_Pendente()
    {
        var etapas = LinhaDoTempo.Montar(NovoPedido());

        Assert.Equal(
            [EstadoEtapa.Atual, EstadoEtapa.Pendente, EstadoEtapa.Pendente, EstadoEtapa.Pendente],
            etapas.Select(e => e.Estado));
    }

    [Fact]
    public void PedidoEnviado_DuasFeitasUmaAtualUmaPendente()
    {
        var pedido = NovoPedido();
        pedido.AlterarStatus(StatusPedido.PagamentoAprovado, OrigemStatus.Automatico);
        pedido.AlterarStatus(StatusPedido.Enviado, OrigemStatus.Automatico);

        var etapas = LinhaDoTempo.Montar(pedido);

        Assert.Equal(
            [EstadoEtapa.Feita, EstadoEtapa.Feita, EstadoEtapa.Atual, EstadoEtapa.Pendente],
            etapas.Select(e => e.Estado));
        Assert.Equal("Enviado", etapas[2].Nome);
    }

    [Fact]
    public void PedidoCancelado_NaoTemEtapaAtual()
    {
        var pedido = NovoPedido();
        pedido.AlterarStatus(StatusPedido.Cancelado, OrigemStatus.Admin);

        var etapas = LinhaDoTempo.Montar(pedido);

        Assert.DoesNotContain(etapas, e => e.Estado == EstadoEtapa.Atual);
        Assert.Equal(EstadoEtapa.Feita, etapas[0].Estado);
    }
}
