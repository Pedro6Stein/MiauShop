using MiauShop.Services.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MiauShop.Areas.Admin.Controllers;

// Base de TODOS os controllers do painel:
// - [Authorize(Roles)]: só usuários com papel Administrador ou Visualizador entram em /Admin.
// - [AutoValidateAntiforgeryToken]: todo POST exige o token antifalsificação.
// - OnActionExecuting: qualquer requisição que altera dados (POST/PUT/DELETE) exige o papel Administrador.
//   É aqui que o Visualizador vira "somente leitura" de verdade, no servidor.
[Area("Admin")]
[Authorize(Roles = Papeis.AcessoAoPainel)]
[AutoValidateAntiforgeryToken]
public abstract class AdminControllerBase : Controller
{
    public const string MensagemSomenteLeitura =
        "Acesso de visualização: você pode navegar por tudo, mas nenhuma alteração é salva.";

    protected bool PodeAlterar => User.IsInRole(Papeis.Administrador);

    public override void OnActionExecuting(ActionExecutingContext contexto)
    {
        var alteraDados = !HttpMethods.IsGet(Request.Method) && !HttpMethods.IsHead(Request.Method);

        if (alteraDados && !PodeAlterar)
        {
            TempData["Aviso"] = MensagemSomenteLeitura;
            contexto.Result = LocalRedirect(PaginaDeOrigem());
            return;
        }

        ViewData["PodeAlterar"] = PodeAlterar;
        base.OnActionExecuting(contexto);
    }

    // Volta para a página anterior, mas só se ela for deste site (evita "open redirect").
    protected string PaginaDeOrigem()
    {
        if (Uri.TryCreate(Request.Headers.Referer.ToString(), UriKind.Absolute, out var origem)
            && origem.Host == Request.Host.Host
            && Url.IsLocalUrl(origem.PathAndQuery))
            return origem.PathAndQuery;

        return "/Admin";
    }
}
