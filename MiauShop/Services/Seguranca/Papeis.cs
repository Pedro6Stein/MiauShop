namespace MiauShop.Services.Seguranca;

// Papéis (roles) do ASP.NET Core Identity usados no painel.
// Relação padrão: Usuario N:N Papel, pelas tabelas AspNetUsers / AspNetUserRoles / AspNetRoles.
public static class Papeis
{
    // Vê e altera tudo no painel.
    public const string Administrador = "Administrador";

    // Só consulta (ex.: sócio ou contador). Também é o papel do acesso de demonstração do portfólio.
    public const string Visualizador = "Visualizador";

    // Para [Authorize(Roles = ...)]: quem pode entrar no painel.
    public const string AcessoAoPainel = Administrador + "," + Visualizador;

    public static readonly string[] Todos = [Administrador, Visualizador];
}
