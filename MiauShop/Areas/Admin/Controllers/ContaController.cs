using System.ComponentModel.DataAnnotations;
using MiauShop.Models.Entities;
using MiauShop.Services.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace MiauShop.Areas.Admin.Controllers;

public class LoginAdmin
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Esse e-mail não parece válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;
}

// Entrada do painel. Fica fora do AdminControllerBase porque precisa abrir sem login.
[Area("Admin")]
[AllowAnonymous]
[AutoValidateAntiforgeryToken]
[FormularioDesatualizado]
public class ContaController(
    SignInManager<Usuario> signInManager,
    UserManager<Usuario> userManager,
    IOptions<OpcoesAdministrador> opcoes,
    ILogger<ContaController> logger) : Controller
{
    public const string PoliticaLogin = "login-admin";
    public const string PoliticaVisitante = "login-visitante";
    private const string MensagemLoginInvalido = "E-mail ou senha incorretos.";

    // GET /Admin/Conta/Entrar
    [HttpGet]
    public IActionResult Entrar(bool bloqueado = false, bool desatualizado = false)
    {
        ViewData["Bloqueado"] = bloqueado;
        ViewData["Desatualizado"] = desatualizado;
        PrepararTela();
        return View(new LoginAdmin());
    }

    // POST /Admin/Conta/Entrar — e-mail e senha.
    [HttpPost]
    [EnableRateLimiting(PoliticaLogin)]
    public async Task<IActionResult> Entrar(LoginAdmin login)
    {
        PrepararTela();
        ViewData["MostrarFormularioAdmin"] = true;

        // Espaços colados sem querer no e-mail não podem impedir o login.
        login.Email = login.Email?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
            return View(login);

        // Só quem tem papel no painel pode entrar. Mensagem genérica: não revela se o e-mail existe.
        var usuario = await userManager.FindByEmailAsync(login.Email);
        if (usuario is null || !await TemAcessoAoPainelAsync(usuario))
        {
            ModelState.AddModelError(string.Empty, MensagemLoginInvalido);
            return View(login);
        }

        // lockoutOnFailure: true -> conta as tentativas erradas e bloqueia após 5.
        var resultado = await signInManager.PasswordSignInAsync(usuario, login.Senha, isPersistent: false, lockoutOnFailure: true);

        if (resultado.IsLockedOut)
        {
            logger.LogWarning("Usuário do painel bloqueado por tentativas: {Email}", login.Email);
            ModelState.AddModelError(string.Empty, "Muitas tentativas erradas. A conta foi bloqueada por 15 minutos.");
            return View(login);
        }

        if (!resultado.Succeeded)
        {
            logger.LogWarning("Senha incorreta no painel para {Email}", login.Email);
            ModelState.AddModelError(string.Empty, MensagemLoginInvalido);
            return View(login);
        }

        await RegistrarAcessoAsync(usuario);
        return RedirectToAction("Index", "Dashboard");
    }

    // POST /Admin/Conta/EntrarComoVisitante — acesso de demonstração (papel Visualizador, sem senha).
    // Só funciona se Administrador:DemoHabilitada = true na configuração.
    // Limite próprio, mais folgado: uma turma inteira na mesma rede (mesmo IP) consegue entrar
    // sem gastar as tentativas do login com senha.
    [HttpPost]
    [EnableRateLimiting(PoliticaVisitante)]
    public async Task<IActionResult> EntrarComoVisitante()
    {
        if (!opcoes.Value.DemoHabilitada)
            return NotFound();

        var demo = await userManager.FindByEmailAsync(opcoes.Value.EmailDemo);
        if (demo is null || !await userManager.IsInRoleAsync(demo, Papeis.Visualizador))
            return RedirectToAction(nameof(Entrar));

        await signInManager.SignInAsync(demo, isPersistent: false);
        await RegistrarAcessoAsync(demo);
        return RedirectToAction("Index", "Dashboard");
    }

    // POST /Admin/Conta/Sair
    [HttpPost]
    public async Task<IActionResult> Sair()
    {
        await signInManager.SignOutAsync();
        return Redirect("/");
    }

    private void PrepararTela()
    {
        ViewData["DemoHabilitada"] = opcoes.Value.DemoHabilitada;

        // Já conectado (ex.: entrou como visitante antes): mostra atalho para o painel.
        if (User.Identity?.IsAuthenticated == true && Papeis.Todos.Any(User.IsInRole))
            ViewData["ConectadoComo"] = User.Identity.Name;
    }

    private async Task<bool> TemAcessoAoPainelAsync(Usuario usuario)
    {
        var papeis = await userManager.GetRolesAsync(usuario);
        return papeis.Any(Papeis.Todos.Contains);
    }

    // "Último acesso" é só informativo: se não der para gravar, o login segue normalmente.
    private async Task RegistrarAcessoAsync(Usuario usuario)
    {
        try
        {
            usuario.RegistrarAcesso();
            var resultado = await userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                logger.LogWarning("Não foi possível registrar o último acesso de {Email}: {Erros}", usuario.Email,
                    string.Join("; ", resultado.Errors.Select(e => e.Description)));
        }
        catch (Exception erro)
        {
            logger.LogWarning(erro, "Não foi possível registrar o último acesso de {Email}", usuario.Email);
        }
    }
}
