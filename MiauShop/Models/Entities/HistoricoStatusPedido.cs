using MiauShop.Models.Enums;

namespace MiauShop.Models.Entities;

// Cada mudança de status vira uma linha: é isso que desenha a linha do tempo do pedido.
// Só é criado pelo próprio Pedido (método AlterarStatus).
public class HistoricoStatusPedido : EntidadeBase
{
    private HistoricoStatusPedido() { }

    internal HistoricoStatusPedido(StatusPedido status, OrigemStatus origem, string? observacao)
    {
        Status = status;
        Origem = origem;
        Observacao = observacao;
    }

    public int PedidoId { get; private set; }
    public Pedido Pedido { get; private set; } = null!;

    public StatusPedido Status { get; private set; }
    public OrigemStatus Origem { get; private set; }
    public string? Observacao { get; private set; }
}
