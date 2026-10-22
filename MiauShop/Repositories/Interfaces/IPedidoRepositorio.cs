using MiauShop.Models.Entities;

namespace MiauShop.Repositories.Interfaces;

public interface IPedidoRepositorio : IRepositorio<Pedido>
{
    // Pedido com itens e histórico. Sem AsNoTracking: o avanço automático pode alterá-lo.
    Task<Pedido?> ObterCompletoAsync(int id);
}
