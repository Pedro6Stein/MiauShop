using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Models.ValueObjects;

namespace MiauShop.Tests;

public class PedidoTests
{
    private static Pedido NovoPedido() =>
        Pedido.Criar("Ana", "Ana@Email.com ", "11999999999", new Endereco(), FormaPagamento.Pix);

    [Fact]
    public void PedidoNovo_NasceRecebidoComHistorico()
    {
        var pedido = NovoPedido();

        Assert.Equal(StatusPedido.Recebido, pedido.Status);
        Assert.Single(pedido.Historico);
        Assert.Equal("ana@email.com", pedido.ClienteEmail);
    }

    [Fact]
    public void Total_SomaItensMaisFrete()
    {
        var produto = new Produto { Nome = "Arranhador", Preco = 60m };
        var variacao = new ProdutoVariacao { Nome = "Único", Produto = produto };
        var pedido = NovoPedido();

        pedido.AdicionarItem(ItemPedido.CriarDe(variacao, 2, "/img/a.webp"));
        pedido.DefinirFrete(19.90m);

        Assert.Equal(120m, pedido.Subtotal);
        Assert.Equal(139.90m, pedido.Total);
    }

    [Fact]
    public void StatusSegueAOrdem_ENaoPulaEtapas()
    {
        var pedido = NovoPedido();

        // Não dá para entregar um pedido que nem foi pago.
        Assert.Throws<InvalidOperationException>(() =>
            pedido.AlterarStatus(StatusPedido.Entregue, OrigemStatus.Admin));

        pedido.AlterarStatus(StatusPedido.PagamentoAprovado, OrigemStatus.Automatico);
        pedido.AlterarStatus(StatusPedido.Enviado, OrigemStatus.Automatico);

        Assert.Equal(StatusPedido.Enviado, pedido.Status);
        Assert.Equal(3, pedido.Historico.Count);
    }

    [Fact]
    public void PedidoEnviado_NaoPodeSerCancelado()
    {
        var pedido = NovoPedido();
        pedido.AlterarStatus(StatusPedido.PagamentoAprovado, OrigemStatus.Automatico);
        pedido.AlterarStatus(StatusPedido.Enviado, OrigemStatus.Automatico);

        Assert.False(pedido.PodeAvancarPara(StatusPedido.Cancelado));
    }
}
