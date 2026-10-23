using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

public class DashboardController(IAdminPedidoService adminPedidoService) : AdminControllerBase
{
    // GET /Admin
    public async Task<IActionResult> Index() => View(await adminPedidoService.ObterDashboardAsync());
}
