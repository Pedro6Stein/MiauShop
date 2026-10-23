using System.ComponentModel.DataAnnotations;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Services;

namespace MiauShop.Areas.Admin.ViewModels;

public record AbaStatus(StatusPedido? Status, string Nome, int Quantidade, bool Ativa);

public class PedidosListaViewModel
{
    public StatusPedido? StatusFiltro { get; init; }
    public string? Busca { get; init; }
    public int Pagina { get; init; }
    public int TotalPaginas { get; init; }
    public int Total { get; init; }
    public List<Pedido> Pedidos { get; init; } = [];
    public List<AbaStatus> Abas { get; init; } = [];

    // URL de uma página/aba mantendo os outros filtros.
    public string Url(StatusPedido? status, int pagina = 1)
    {
        var partes = new List<string>();
        if (status is not null) partes.Add($"status={status}");
        if (!string.IsNullOrWhiteSpace(Busca)) partes.Add($"busca={Uri.EscapeDataString(Busca)}");
        if (pagina > 1) partes.Add($"pagina={pagina}");
        return "/Admin/Pedidos" + (partes.Count > 0 ? "?" + string.Join("&", partes) : "");
    }
}

public record LinhaHistorico(string Status, DateTime Quando, string Origem, string? Observacao);

public class PedidoDetalheAdminViewModel
{
    public required Pedido Pedido { get; init; }
    public required List<EtapaPedido> Etapas { get; init; }
    public required List<LinhaHistorico> Historico { get; init; }
    public DateTime CriadoEm { get; init; }

    // Próximos status oferecidos no seletor (sem "Cancelado", que tem botão próprio).
    public List<StatusPedido> ProximosStatus { get; init; } = [];
    public bool PodeCancelar { get; init; }

    public AlteracaoStatus Alteracao { get; init; } = new();
}

// Formulário "Alterar status". Versao = versão do pedido quando a tela foi aberta (concorrência).
public class AlteracaoStatus
{
    [Required(ErrorMessage = "Escolha o novo status.")]
    public StatusPedido? NovoStatus { get; set; }

    [StringLength(300, ErrorMessage = "Use no máximo 300 caracteres.")]
    public string? Observacao { get; set; }

    public uint Versao { get; set; }
}
