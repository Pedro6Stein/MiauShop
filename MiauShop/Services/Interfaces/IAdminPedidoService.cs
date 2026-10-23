using MiauShop.Areas.Admin.ViewModels;

namespace MiauShop.Services.Interfaces;

// Regras do painel para pedidos (dashboard agora; lista e mudança de status na próxima entrega).
public interface IAdminPedidoService
{
    Task<DashboardViewModel> ObterDashboardAsync();
}
