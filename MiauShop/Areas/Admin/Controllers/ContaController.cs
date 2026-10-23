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
public class ContaController(
    SignInManager<Usuario> signInManager,
    UserManager<Usuario> userManager,
    IOptions<OpcoesAdministrador> opcoes,
    ILogger<ContaController> logger) : Controller
{
    public const string PoliticaLogin = "login-admin";
    private const string MensagemLoginInvalido = "E-mail ou senha incorretos.";

    // GET /Admin/Conta/Entrar
    [HttpGet]
    public IActionResult Entrar(bool bloqueado = false)
    {
        ViewData["Bloqueado"] = bloqueado;
        ViewData["DemoHabilitada"] = opcoes.Value.DemoHabilitada;
        return View(new LoginAdmin());
    }

    // POST /Admin/Conta/Entrar — e-mail e senha.
    [HttpPost]
    [EnableRateLimiting(PoliticaLogin)]
    public async Task<IActionResult> Entrar(LoginAdmin login)
    {
        ViewData["DemoHabilitada"] = opcoes.Value.DemoHabilitada;

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
    [HttpPost]
    [EnableRateLimiting(PoliticaLogin)]
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

    private async Task<bool> TemAcessoAoPainelAsync(Usuario usuario)
    {
        var papeis = await userManager.GetRolesAsync(usuario);
        return papeis.Any(Papeis.Todos.Contains);
    }

    private async Task RegistrarAcessoAsync(Usuario usuario)
    {
        usuario.RegistrarAcesso();
        await userManager.UpdateAsync(usuario);
    }
}
