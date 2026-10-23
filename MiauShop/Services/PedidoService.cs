using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class PedidoService(IPedidoRepositorio pedidoRepositorio, AvancoAutomatico avancoAutomatico) : IPedidoService
{
    public Task<Pedido?> ObterAsync(int pedidoId) =>
        pedidoRepositorio.ObterCompletoAsync(pedidoId);

    public async Task<Pedido?> ObterParaAcompanhamentoAsync(int pedidoId, string email)
    {
        var pedido = await pedidoRepositorio.ObterCompletoAsync(pedidoId);

        // Número certo com e-mail errado se comporta como "não encontrado".
        if (pedido is null || pedido.ClienteEmail != email.Trim().ToLowerInvariant())
            return null;

        // Em vez de um processo em segundo plano (que para quando o servidor grátis dorme),
        // o status é atualizado quando alguém abre o pedido (se o recurso estiver ligado).
        if (avancoAutomatico.Aplicar(pedido, DateTime.UtcNow))
            await pedidoRepositorio.SalvarAsync();

        return pedido;
    }
}
