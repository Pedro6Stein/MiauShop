using System.ComponentModel.DataAnnotations;

namespace MiauShop.Models.Enums;

// Todas as formas são simuladas: nenhum pagamento real é processado.
public enum FormaPagamento
{
    [Display(Name = "Pix")]
    Pix,

    [Display(Name = "Cartão de crédito")]
    Cartao,

    [Display(Name = "Boleto")]
    Boleto
}
