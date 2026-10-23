using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Repositories.Interfaces;

public interface IPedidoRepositorio : IRepositorio<Pedido>
{
    // Pedido com itens e histórico. Sem AsNoTracking: o avanço automático pode alterá-lo.
    Task<Pedido?> ObterCompletoAsync(int id);

    // Números do dashboard: pedidos por status, faturamento, mais vendidos e últimos pedidos.
    Task<ResumoPedidos> ObterResumoAsync(int quantidadeMaisVendidos, int quantidadeUltimos);

    Task<Dictionary<StatusPedido, int>> ContarPorStatusAsync();

    // Lista do painel: filtro por status, busca por número/nome/e-mail, paginação (mais novos primeiro).
    Task<(List<Pedido> Itens, int Total)> ListarParaPainelAsync(StatusPedido? status, string? busca, int pagina, int itensPorPagina);

    // Controle de concorrência: informa a versão que o administrador tinha na tela.
    // Se o pedido mudou depois disso, o SalvarAsync falha com DbUpdateConcurrencyException.
    void DefinirVersaoOriginal(Pedido pedido, uint versao);
}
