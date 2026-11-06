using System.Security.Claims;
using MiauShop.Services.Seguranca;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MiauShop.Tests;

// Token antifalsificação que não confere no painel: em vez da página em branco (400), redireciona com aviso.
public class FormularioDesatualizadoTests
{
    [Fact]
    public void NaTelaDeLogin_TokenRecusado_VoltaParaOLoginComAviso()
    {
        var contexto = Contexto("Conta", autenticado: true, new AntiforgeryValidationFailedResult());

        new FormularioDesatualizadoAttribute().OnResultExecuting(contexto);

        var redirecionamento = Assert.IsType<RedirectResult>(contexto.Result);
        Assert.Equal("/Admin/Conta/Entrar?desatualizado=true", redirecionamento.Url);
    }

    [Fact]
    public void NoPainel_Logado_VoltaParaAPaginaDeOrigem()
    {
        var contexto = Contexto("Produtos", autenticado: true, new AntiforgeryValidationFailedResult(),
            referer: "http://localhost/Admin/Produtos/Editar/3");

        new FormularioDesatualizadoAttribute().OnResultExecuting(contexto);

        var redirecionamento = Assert.IsType<LocalRedirectResult>(contexto.Result);
        Assert.Equal("/Admin/Produtos/Editar/3", redirecionamento.Url);
    }

    [Fact]
    public void RefererDeOutroSite_NaoEhUsado()
    {
        var contexto = Contexto("Produtos", autenticado: true, new AntiforgeryValidationFailedResult(),
            referer: "https://site-malicioso.com/Admin/qualquer");

        new FormularioDesatualizadoAttribute().OnResultExecuting(contexto);

        Assert.Equal("/Admin", Assert.IsType<LocalRedirectResult>(contexto.Result).Url);
    }

    [Fact]
    public void OutrosResultados_NaoSaoAlterados()
    {
        var original = new ViewResult();
        var contexto = Contexto("Conta", autenticado: false, original);

        new FormularioDesatualizadoAttribute().OnResultExecuting(contexto);

        Assert.Same(original, contexto.Result);
    }

    private static ResultExecutingContext Contexto(string controller, bool autenticado, IActionResult resultado, string? referer = null)
    {
        var servicos = new ServiceCollection()
            .AddSingleton<ITempDataDictionaryFactory, TempDataFalso>()
            .BuildServiceProvider();

        var http = new DefaultHttpContext { RequestServices = servicos };
        http.Request.Host = new HostString("localhost");
        if (referer is not null)
            http.Request.Headers.Referer = referer;
        if (autenticado)
            http.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin@teste.local")], "Teste"));

        var rotas = new RouteData();
        rotas.Values["controller"] = controller;

        var acao = new ActionContext(http, rotas, new ActionDescriptor());
        return new ResultExecutingContext(acao, [], resultado, controller: new object());
    }

    private class TempDataFalso : ITempDataDictionaryFactory
    {
        public ITempDataDictionary GetTempData(HttpContext context) =>
            new TempDataDictionary(context, new ProvedorTempDataFalso());
    }

    private class ProvedorTempDataFalso : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
