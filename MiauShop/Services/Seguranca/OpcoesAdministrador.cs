namespace MiauShop.Services.Seguranca;

// Seção "Administrador" da configuração. E-mail e senha do administrador vêm de
// variáveis de ambiente no Render (Administrador__Email / Administrador__Senha) ou de
// user secrets na máquina de desenvolvimento. Nunca do código nem do GitHub.
public class OpcoesAdministrador
{
    public const string Secao = "Administrador";

    public string? Email { get; set; }
    public string? Senha { get; set; }
    public string Nome { get; set; } = "Administrador";

    // Acesso de demonstração do portfólio: botão "Entrar como visitante", que entra com um usuário
    // de papel Visualizador (somente leitura). Para uso real da loja, basta desligar (false).
    public bool DemoHabilitada { get; set; } = true;
    public string EmailDemo { get; set; } = "demo@miaushop.local";

    public bool AdministradorConfigurado =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Senha);
}
