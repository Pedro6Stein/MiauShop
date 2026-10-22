using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MiauShop.Models.Enums;

public static class EnumExtensions
{
    // Lê o [Display(Name = "...")] do valor: Animal.Cao -> "Cães".
    public static string NomeExibicao(this Enum valor) =>
        valor.GetType().GetMember(valor.ToString()).FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>()?.GetName()
        ?? valor.ToString();
}
