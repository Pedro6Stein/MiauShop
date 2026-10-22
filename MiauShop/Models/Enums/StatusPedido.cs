using System.ComponentModel.DataAnnotations;

namespace MiauShop.Models.Enums;

public enum StatusPedido
{
    [Display(Name = "Recebido")]
    Recebido,

    [Display(Name = "Pagamento aprovado")]
    PagamentoAprovado,

    [Display(Name = "Enviado")]
    Enviado,

    [Display(Name = "Entregue")]
    Entregue,

    [Display(Name = "Cancelado")]
    Cancelado
}
