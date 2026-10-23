using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Services;

public class AdminPedidoService(IPedidoRepositorio pedidoRepositorio, AvancoAutomatico avancoAutomatico) : IAdminPedidoService
{
    private const int QuantidadeMaisVendidos = 5;
    private const int QuantidadeUltimos = 5;
    public const int ItensPorPagina = 20;

    public async Task<DashboardViewModel> ObterDashboardAsync() =>
        DashboardViewModel.Montar(await pedidoRepositorio.ObterResumoAsync(QuantidadeMaisVendidos, QuantidadeUltimos));

    public async Task<PedidosListaViewModel> ListarAsync(StatusPedido? status, string? busca, int pagina)
    {
        pagina = Math.Max(pagina, 1);
        var (pedidos, total) = await pedidoRepositorio.ListarParaPainelAsync(status, busca, pagina, ItensPorPagina);

        // Se o avanço automático (demonstração) estiver ligado, atualiza os pedidos desta página.
        var agora = DateTime.UtcNow;
        if (pedidos.Select(p => avancoAutomatico.Aplicar(p, agora)).ToList().Any(mudou => mudou))
            await pedidoRepositorio.SalvarAsync();

        var porStatus = await pedidoRepositorio.ContarPorStatusAsync();
        var abas = new List<AbaStatus> { new(null, "Todos", porStatus.Values.Sum(), status is null) };
        abas.AddRange(Enum.GetValues<StatusPedido>()
            .Select(s => new AbaStatus(s, s.NomeExibicao(), porStatus.GetValueOrDefault(s), status == s)));

        return new PedidosListaViewModel
        {
            StatusFiltro = status,
            Busca = busca,
            Pagina = pagina,
            Total = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)ItensPorPagina),
            Pedidos = pedidos,
            Abas = abas
        };
    }

    public async Task<PedidoDetalheAdminViewModel?> ObterDetalheAsync(int id)
    {
        var pedido = await pedidoRepositorio.ObterCompletoAsync(id);
        if (pedido is null)
            return null;

        if (avancoAutomatico.Aplicar(pedido, DateTime.UtcNow))
            await pedidoRepositorio.SalvarAsync();

        var proximos = pedido.ProximosStatusPossiveis();

        return new PedidoDetalheAdminViewModel
        {
            Pedido = pedido,
            Etapas = LinhaDoTempo.Montar(pedido),
            CriadoEm = LinhaDoTempo.ParaBrasilia(pedido.CriadoEm),
            Historico = pedido.Historico
                .OrderByDescending(h => h.CriadoEm)
                .Select(h => new LinhaHistorico(h.Status.NomeExibicao(), LinhaDoTempo.ParaBrasilia(h.CriadoEm), h.Origem.NomeExibicao(), h.Observacao))
                .ToList(),
            ProximosStatus = proximos.Where(s => s != StatusPedido.Cancelado).ToList(),
            PodeCancelar = proximos.Contains(StatusPedido.Cancelado),
            Alteracao = new AlteracaoStatus { Versao = pedido.Versao }
        };
    }

    public async Task<string?> AlterarStatusAsync(int id, AlteracaoStatus alteracao)
    {
        var pedido = await pedidoRepositorio.ObterCompletoAsync(id);
        if (pedido is null)
            return "Pedido não encontrado.";

        // Compara com a versão que o administrador estava vendo na tela.
        pedidoRepositorio.DefinirVersaoOriginal(pedido, alteracao.Versao);

        try
        {
            // As regras (transição permitida, motivo obrigatório no cancelamento) moram no próprio Pedido.
            pedido.AlterarStatus(alteracao.NovoStatus!.Value, OrigemStatus.Admin, alteracao.Observacao);
            await pedidoRepositorio.SalvarAsync();
            return null;
        }
        catch (InvalidOperationException erro)
        {
            return erro.Message;
        }
        catch (DbUpdateConcurrencyException)
        {
            return "Este pedido foi alterado por outra pessoa enquanto você estava na tela. Confira o status atual e tente de novo.";
        }
    }
}
