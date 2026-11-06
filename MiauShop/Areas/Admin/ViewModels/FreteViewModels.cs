using System.ComponentModel.DataAnnotations;
using MiauShop.Models;

namespace MiauShop.Areas.Admin.ViewModels;

public class FreteFormulario
{
    [Display(Name = "CEP de origem (de onde os pedidos saem)")]
    [Required(ErrorMessage = "Informe o CEP de origem.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Use o formato 00000-000.")]
    public string CepOrigem { get; set; } = string.Empty;

    [Display(Name = "Valor base (R$)")]
    [Required(ErrorMessage = "Informe o valor base.")]
    [Range(0, 999, ErrorMessage = "Use de R$ 0 a R$ 999.")]
    public decimal? ValorBase { get; set; }

    [Display(Name = "Valor por km (R$)")]
    [Required(ErrorMessage = "Informe o valor por km.")]
    [Range(0, 10, ErrorMessage = "Use de R$ 0 a R$ 10 por km.")]
    public decimal? ValorPorKm { get; set; }

    [Display(Name = "Valor máximo do frete (R$)")]
    [Required(ErrorMessage = "Informe o valor máximo.")]
    [Range(0, 9999, ErrorMessage = "Use de R$ 0 a R$ 9.999.")]
    public decimal? ValorMaximo { get; set; }
}

public class FreteViewModel
{
    public required FreteFormulario Formulario { get; init; }
    public string CidadeOrigem { get; init; } = string.Empty;
    public string UfOrigem { get; init; } = string.Empty;
    public decimal MinimoParaFreteGratis { get; init; }

    // Simulador: frete da tabela para um CEP de destino (sem considerar o frete grátis).
    public string? CepSimulado { get; init; }
    public CotacaoFrete? Simulacao { get; init; }
}
