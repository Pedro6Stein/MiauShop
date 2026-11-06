using MiauShop.Models.Entities;

namespace MiauShop.Areas.Admin.ViewModels;

public class AvaliacoesViewModel
{
    public const string Pendentes = "pendentes";
    public const string Aprovadas = "aprovadas";
    public const string Todas = "todas";

    public string Situacao { get; init; } = Pendentes;
    public List<AvaliacaoProduto> Avaliacoes { get; init; } = [];
    public int QuantidadePendentes { get; init; }
}
