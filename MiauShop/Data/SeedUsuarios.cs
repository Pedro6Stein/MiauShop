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
        // Espaços nas pontas (comuns ao colar no painel do Render) são removidos.
        if (opcoes.AdministradorConfigurado)
            await GarantirUsuarioAsync(userManager, opcoes.Email!.Trim(), opcoes.Nome, opcoes.Senha!.Trim(), Papeis.Administrador, logger);
        else
            logger.LogWarning("Administrador não configurado: defina Administrador__Email e Administrador__Senha no servidor.");

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
                // Causa comum: senha fora das regras (mín. 10 caracteres, maiúscula, minúscula, número e símbolo).
                logger.LogError("Não foi possível criar o usuário {Email}: {Erros}", email, Erros(criacao.Errors));
                return;
            }
        }
        else if (senha is not null)
        {
            await SincronizarSenhaAsync(userManager, usuario, senha, logger);
        }

        if (!await userManager.IsInRoleAsync(usuario, papel))
            await userManager.AddToRoleAsync(usuario, papel);
    }

    // A senha do administrador vem da configuração do servidor (Render). Antes, ela só era usada
    // na PRIMEIRA criação do usuário: trocar a variável depois não mudava nada no banco e o login
    // com a senha nova dava "E-mail ou senha incorretos". Agora a configuração manda.
    private static async Task SincronizarSenhaAsync(UserManager<Usuario> userManager, Usuario usuario, string senha, ILogger logger)
    {
        if (await userManager.CheckPasswordAsync(usuario, senha))
            return;

        // Valida ANTES de remover a senha antiga, para nunca deixar o usuário sem senha.
        var erros = new List<IdentityError>();
        foreach (var validador in userManager.PasswordValidators)
        {
            var validacao = await validador.ValidateAsync(userManager, usuario, senha);
            erros.AddRange(validacao.Errors);
        }

        if (erros.Count > 0)
        {
            logger.LogError("A senha configurada para {Email} não atende às regras e foi ignorada: {Erros}", usuario.Email, Erros(erros));
            return;
        }

        await userManager.RemovePasswordAsync(usuario);
        var troca = await userManager.AddPasswordAsync(usuario, senha);
        if (!troca.Succeeded)
        {
            logger.LogError("Não foi possível atualizar a senha de {Email}: {Erros}", usuario.Email, Erros(troca.Errors));
            return;
        }

        // Senha nova: zera tentativas erradas e desbloqueia (a pessoa pode ter errado tentando a senha nova).
        await userManager.ResetAccessFailedCountAsync(usuario);
        await userManager.SetLockoutEndDateAsync(usuario, null);
        logger.LogInformation("Senha de {Email} atualizada a partir da configuração do servidor.", usuario.Email);
    }

    private static string Erros(IEnumerable<IdentityError> erros) =>
        string.Join("; ", erros.Select(e => e.Description));
}
