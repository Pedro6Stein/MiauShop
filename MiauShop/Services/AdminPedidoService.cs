using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class AdminPedidoService(IPedidoRepositorio pedidoRepositorio) : IAdminPedidoService
{
    private const int QuantidadeMaisVendidos = 5;
    private const int QuantidadeUltimos = 5;

    public async Task<DashboardViewModel> ObterDashboardAsync() =>
        DashboardViewModel.Montar(await pedidoRepositorio.ObterResumoAsync(QuantidadeMaisVendidos, QuantidadeUltimos));
}
