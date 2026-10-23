using System.ComponentModel.DataAnnotations;
using MiauShop.Models.Entities;
using MiauShop.Services;

namespace MiauShop.Models.ViewModels;

// Formulário de busca (número + e-mail).
public class BuscaPedido
{
    [Display(Name = "Número do pedido")]
    [Required(ErrorMessage = "Informe o número do pedido.")]
    [Range(1, int.MaxValue, ErrorMessage = "Número inválido.")]
    public int? Numero { get; set; }

    [Display(Name = "E-mail usado na compra")]
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Esse e-mail não parece válido.")]
    public string Email { get; set; } = string.Empty;
}

public class AcompanhamentoViewModel
{
    public BuscaPedido Busca { get; init; } = new();

    // Preenchidos só quando a busca encontra o pedido.
    public Pedido? Pedido { get; init; }
    public List<EtapaPedido> Etapas { get; init; } = [];
    public DateTime? CriadoEm { get; init; }
    public DateTime? CanceladoEm { get; init; }
    public string? MotivoCancelamento { get; init; }
    public bool AvancoAutomaticoHabilitado { get; init; }

    public bool BuscaFeita { get; init; }
    public bool Bloqueado { get; init; }
}
