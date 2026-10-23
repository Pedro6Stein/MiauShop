using MiauShop.Models;
using MiauShop.Models.Entities;

namespace MiauShop.Repositories.Interfaces;

public interface IPedidoRepositorio : IRepositorio<Pedido>
{
    // Pedido com itens e histórico. Sem AsNoTracking: o avanço automático pode alterá-lo.
    Task<Pedido?> ObterCompletoAsync(int id);

    // Números do dashboard: pedidos por status, faturamento, mais vendidos e últimos pedidos.
    Task<ResumoPedidos> ObterResumoAsync(int quantidadeMaisVendidos, int quantidadeUltimos);
}
