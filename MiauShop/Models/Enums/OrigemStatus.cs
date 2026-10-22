using System.ComponentModel.DataAnnotations;

namespace MiauShop.Models.Enums;

// Quem provocou a mudança de status (aparece no histórico do pedido).
public enum OrigemStatus
{
    [Display(Name = "Sistema")]
    Sistema,

    [Display(Name = "Automático")]
    Automatico,

    [Display(Name = "Administrador")]
    Admin
}
