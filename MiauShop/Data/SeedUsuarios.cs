using MiauShop.Models.Entities;
using MiauShop.Services.Seguranca;
using Microsoft.AspNetCore.Identity;

namespace MiauShop.Data;

// Cria os papéis do painel e os usuários iniciais na inicialização.
// Não existe cadastro público de usuários do painel.
public static class SeedUsuarios
{
    public static async Task InicializarAsync(
        UserManager<Usuario> userManager,
        RoleManager<IdentityRole> roleManager,
        OpcoesAdministrador opcoes,
        ILogger logger)
    {
        foreach (var papel in Papeis.Todos)
            if (!await roleManager.RoleExistsAsync(papel))
                await roleManager.CreateAsync(new IdentityRole(papel));

        // Administrador: só se e-mail e senha estiverem configurados no servidor.
        // A senha passa pelas regras de força do Identity e é gravada como hash (PBKDF2 + salt).
        if (opcoes.AdministradorConfigurado)
            await GarantirUsuarioAsync(userManager, opcoes.Email!, opcoes.Nome, opcoes.Senha, Papeis.Administrador, logger);
        else
            logger.LogInformation("Administrador não configurado (Administrador:Email / Administrador:Senha).");

        // Demonstração do portfólio: usuário Visualizador SEM senha. Não dá para entrar nele pelo
        // formulário de e-mail e senha; só pelo botão "Entrar como visitante", que existe se DemoHabilitada = true.
        if (opcoes.DemoHabilitada)
            await GarantirUsuarioAsync(userManager, opcoes.EmailDemo, "Visitante", senha: null, Papeis.Visualizador, logger);
    }

    private static async Task GarantirUsuarioAsync(
        UserManager<Usuario> userManager, string email, string nome, string? senha, string papel, ILogger logger)
    {
        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario is null)
        {
            usuario = new Usuario { UserName = email, Email = email, EmailConfirmed = true, Nome = nome };
            var criacao = senha is null
                ? await userManager.CreateAsync(usuario)
                : await userManager.CreateAsync(usuario, senha);

            if (!criacao.Succeeded)
            {
                logger.LogError("Não foi possível criar o usuário {Email}: {Erros}", email,
                    string.Join("; ", criacao.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(usuario, papel))
            await userManager.AddToRoleAsync(usuario, papel);
    }
}
