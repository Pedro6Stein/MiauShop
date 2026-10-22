using System.ComponentModel.DataAnnotations;

namespace MiauShop.Models.Enums;

public enum Animal
{
    [Display(Name = "Cães")]
    Cao,

    [Display(Name = "Gatos")]
    Gato,

    [Display(Name = "Cães e gatos")]
    Ambos
}
