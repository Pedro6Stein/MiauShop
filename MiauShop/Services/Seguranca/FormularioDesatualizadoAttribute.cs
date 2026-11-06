using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MiauShop.Services.Seguranca;

// Quando o token antifalsificação de um formulário do painel não confere, o ASP.NET Core
// responde "400 Bad Request" com uma página em branco. Isso acontece, por exemplo, quando a pessoa
// entra ou sai em outra aba depois de abrir o formulário (o token foi gerado para outro usuário).
// Aqui trocamos a tela em branco por um aviso e um novo formulário. A proteção continua igual:
// nada do POST rejeitado é processado.
[AttributeUsage(AttributeTargets.Class)]
public class FormularioDesatualizadoAttribute : Attribute, IAlwaysRunResultFilter
{
    public const string Mensagem = "A página estava desatualizada (você entrou ou saiu em outra aba). Tente de novo.";

    public void OnResultExecuting(ResultExecutingContext contexto)
    {
        if (contexto.Result is not IAntiforgeryValidationFailedResult)
            return;

        var http = contexto.HttpContext;
        var naTelaDeEntrada = string.Equals(
            contexto.RouteData.Values["controller"]?.ToString(), "Conta", StringComparison.OrdinalIgnoreCase);

        if (naTelaDeEntrada || http.User.Identity?.IsAuthenticated != true)
        {
            contexto.Result = new RedirectResult("/Admin/Conta/Entrar?desatualizado=true");
            return;
        }

        // Já está logado: volta para a página do painel onde estava, com o aviso.
        var tempData = http.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(http);
        tempData["Erro"] = Mensagem;
        contexto.Result = new LocalRedirectResult(PaginaDoPainelDeOrigem(http));
    }

    public void OnResultExecuted(ResultExecutedContext contexto) { }

    // Só volta para páginas do próprio painel (evita "open redirect").
    private static string PaginaDoPainelDeOrigem(HttpContext http)
    {
        if (Uri.TryCreate(http.Request.Headers.Referer.ToString(), UriKind.Absolute, out var origem)
            && origem.Host == http.Request.Host.Host
            && origem.AbsolutePath.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return origem.PathAndQuery;

        return "/Admin";
    }
}
