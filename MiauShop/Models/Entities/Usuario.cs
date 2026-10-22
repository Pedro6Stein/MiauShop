using Microsoft.AspNetCore.Identity;

namespace MiauShop.Models.Entities;

// Herda do IdentityUser: e-mail, senha (com hash), bloqueio etc. vêm prontos.
// Não herda de EntidadeBase porque o Id do Identity é string, não int.
public class Usuario : IdentityUser
{
    public string Nome { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
