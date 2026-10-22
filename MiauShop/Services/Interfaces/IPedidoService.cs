using MiauShop.Models.Entities;

namespace MiauShop.Services.Interfaces;

public interface IPedidoService
{
    // Para a tela de confirmação, logo após a compra.
    Task<Pedido?> ObterAsync(int pedidoId);

    // Para o cliente acompanhar: exige número + e-mail e atualiza o status pelo tempo.
    Task<Pedido?> ObterParaAcompanhamentoAsync(int pedidoId, string email);
}
