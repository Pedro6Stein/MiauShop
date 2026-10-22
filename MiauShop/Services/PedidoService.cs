using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class PedidoService(IPedidoRepositorio pedidoRepositorio) : IPedidoService
{
    // Demonstração: quanto tempo depois da compra cada status "acontece".
    private static readonly (StatusPedido Status, TimeSpan Apos, string Observacao)[] Etapas =
    [
        (StatusPedido.PagamentoAprovado, TimeSpan.FromMinutes(1), "Pagamento aprovado (simulação)."),
        (StatusPedido.Enviado, TimeSpan.FromMinutes(5), "Pedido enviado pelo fornecedor."),
        (StatusPedido.Entregue, TimeSpan.FromMinutes(15), "Pedido entregue. Aproveite!")
    ];

    public Task<Pedido?> ObterAsync(int pedidoId) =>
        pedidoRepositorio.ObterCompletoAsync(pedidoId);

    public async Task<Pedido?> ObterParaAcompanhamentoAsync(int pedidoId, string email)
    {
        var pedido = await pedidoRepositorio.ObterCompletoAsync(pedidoId);

        // Número certo com e-mail errado se comporta como "não encontrado".
        if (pedido is null || pedido.ClienteEmail != email.Trim().ToLowerInvariant())
            return null;

        await AvancarAutomaticamenteAsync(pedido);
        return pedido;
    }

    // Em vez de um processo rodando em segundo plano (que para quando o servidor grátis dorme),
    // o status é atualizado quando alguém abre o pedido.
    private async Task AvancarAutomaticamenteAsync(Pedido pedido)
    {
        var mudou = false;

        foreach (var etapa in Etapas)
        {
            var quando = pedido.CriadoEm + etapa.Apos;

            // PodeAvancarPara impede, por exemplo, avançar um pedido cancelado pelo admin.
            if (DateTime.UtcNow >= quando && pedido.PodeAvancarPara(etapa.Status))
            {
                pedido.AlterarStatus(etapa.Status, OrigemStatus.Automatico, etapa.Observacao, quando);
                mudou = true;
            }
        }

        if (mudou)
            await pedidoRepositorio.SalvarAsync();
    }
}
