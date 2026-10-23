using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Models.Enums;

namespace MiauShop.Services.Interfaces;

// Regras do painel para pedidos.
public interface IAdminPedidoService
{
    Task<DashboardViewModel> ObterDashboardAsync();

    Task<PedidosListaViewModel> ListarAsync(StatusPedido? status, string? busca, int pagina);

    Task<PedidoDetalheAdminViewModel?> ObterDetalheAsync(int id);

    // Devolve null se deu certo, ou a mensagem de erro para mostrar na tela.
    Task<string?> AlterarStatusAsync(int id, AlteracaoStatus alteracao);
}
