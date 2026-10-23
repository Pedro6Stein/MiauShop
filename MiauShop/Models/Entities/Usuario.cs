using Microsoft.AspNetCore.Identity;

namespace MiauShop.Models.Entities;

// Usuário do sistema. Herda do IdentityUser: e-mail, hash da senha, bloqueio por tentativas etc.
// O que cada usuário pode fazer vem dos PAPÉIS do Identity (tabelas AspNetRoles e AspNetUserRoles),
// não de colunas nesta classe. Ver Services/Seguranca/Papeis.cs.
public class Usuario : IdentityUser
{
    public string Nome { get; set; } = string.Empty;

    public DateTime? UltimoAcessoEm { get; set; }

    // Pedidos feitos por este usuário como cliente (hoje a loja aceita compra sem conta, então fica vazio).
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public void RegistrarAcesso() => UltimoAcessoEm = DateTime.UtcNow;
}
