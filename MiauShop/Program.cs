using MiauShop.Data;
using MiauShop.Models.Entities;
using MiauShop.Repositories;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using MiauShop.Services.Seguranca;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Banco de dados: a string de conexão vem do appsettings (local) ou de variável de ambiente (produção).
// EnableRetryOnFailure: o Neon gratuito "dorme" o banco após alguns minutos parado e a primeira
// conexão pode falhar enquanto ele acorda; o EF tenta de novo sozinho em vez de mostrar erro.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MiauShop"),
        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

// ---------- Hospedagem (Render) ----------

// Chaves do cookie de login e do token antifalsificação ficam no banco: o disco do Render é apagado
// a cada reinício, e sem isso todo mundo seria deslogado e formulários abertos dariam erro.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("MiauShop");

// O Render recebe o HTTPS e repassa ao site em HTTP, avisando nos cabeçalhos X-Forwarded-*.
// Confiar nesses cabeçalhos faz o site saber que a conexão original é HTTPS (cookie seguro,
// redirecionamentos) e enxergar o IP real do visitante (usado pelo limite de tentativas).
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(opcoes =>
{
    opcoes.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                            | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    // O proxy do Render não tem IP fixo conhecido; o site só é acessível através dele.
    opcoes.KnownIPNetworks.Clear();
    opcoes.KnownProxies.Clear();
});

// Rota /saude: o Render consulta para saber se o site subiu.
builder.Services.AddHealthChecks();

// ---------- Login do painel (ASP.NET Core Identity) ----------
// Senhas: o Identity grava só o HASH (PBKDF2 com HMAC-SHA512, 100 mil iterações, salt aleatório por usuário).
builder.Services.AddIdentity<Usuario, IdentityRole>(opcoes =>
    {
        opcoes.Password.RequiredLength = 10;
        opcoes.Password.RequireUppercase = true;
        opcoes.Password.RequireLowercase = true;
        opcoes.Password.RequireDigit = true;
        opcoes.Password.RequireNonAlphanumeric = true;

        // 5 senhas erradas seguidas bloqueiam a conta por 15 minutos.
        opcoes.Lockout.MaxFailedAccessAttempts = 5;
        opcoes.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

        opcoes.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(opcoes =>
{
    opcoes.Cookie.Name = "MiauShop.Admin";
    opcoes.Cookie.HttpOnly = true;                       // JavaScript não lê o cookie
    // Lax (padrão do Identity): o cookie vai junto quando a pessoa chega por um link de outro site
    // (README do GitHub, WhatsApp...). Com Strict, quem já estava logado e chegava por link recebia o
    // formulário gerado como "anônimo" e o envio quebrava com erro 400 do token antifalsificação.
    // POSTs vindos de outros sites continuam bloqueados (Lax não envia o cookie neles + token antifalsificação).
    opcoes.Cookie.SameSite = SameSiteMode.Lax;
    opcoes.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;                     // em produção, só por HTTPS
    opcoes.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    opcoes.SlidingExpiration = true;
    opcoes.LoginPath = "/Admin/Conta/Entrar";
    opcoes.AccessDeniedPath = "/Admin/Conta/Entrar";
});

builder.Services.Configure<OpcoesAdministrador>(builder.Configuration.GetSection(OpcoesAdministrador.Secao));

// Sessão: guarda o carrinho do visitante (expira após 2 horas sem uso).
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Repositórios: "quando alguém pedir a interface, entregue esta classe".
// AddScoped = uma instância por requisição, todas usando o mesmo AppDbContext.
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));
builder.Services.AddScoped<IProdutoRepositorio, ProdutoRepositorio>();
builder.Services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
builder.Services.AddScoped<IAvaliacaoRepositorio, AvaliacaoRepositorio>();
builder.Services.AddScoped<IPerguntaSemRespostaRepositorio, PerguntaSemRespostaRepositorio>();

// Serviços
builder.Services.Configure<OpcoesFrete>(builder.Configuration.GetSection(OpcoesFrete.Secao));
builder.Services.AddSingleton<ICalculadoraFrete, FreteFixoComGratis>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<ICarrinhoService, CarrinhoService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IAdminPedidoService, AdminPedidoService>();
builder.Services.AddScoped<IAdminCatalogoService, AdminCatalogoService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IAssistenteService, AssistenteService>();

// Avanço automático de status (demonstração; desligável na configuração).
builder.Services.Configure<OpcoesAvancoAutomatico>(builder.Configuration.GetSection(OpcoesAvancoAutomatico.Secao));
builder.Services.AddSingleton<AvancoAutomatico>();

// Checkout: Public Key de teste do Mercado Pago (user secrets / variável de ambiente) e consulta de CEP.
builder.Services.Configure<OpcoesMercadoPago>(builder.Configuration.GetSection(OpcoesMercadoPago.Secao));
builder.Services.AddHttpClient<IConsultaCep, ViaCepService>(cliente =>
{
    cliente.BaseAddress = new Uri("https://viacep.com.br/ws/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

// Frete por distância: coordenada do CEP pela AwesomeAPI (gratuita, sem chave), guardada 24 h em memória.
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ILocalizacaoCep, LocalizacaoCepService>(cliente =>
{
    cliente.BaseAddress = new Uri("https://cep.awesomeapi.com.br/json/");
    cliente.Timeout = TimeSpan.FromSeconds(4);
});
builder.Services.AddScoped<IFreteService, FreteService>();

// Limitador de tentativas (recurso nativo do ASP.NET Core): busca de pedido aceita
// no máximo 10 tentativas por minuto por IP; passou disso, volta para a tela com aviso.
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.AddPolicy(MiauShop.Controllers.PedidoController.PoliticaBusca, contexto =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Login do painel: 5 tentativas por minuto por IP (além do bloqueio da conta pelo Identity).
    opcoes.AddPolicy(MiauShop.Areas.Admin.Controllers.ContaController.PoliticaLogin, contexto =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Botão "Entrar como visitante": limite separado e mais folgado (turma inteira na mesma rede).
    opcoes.AddPolicy(MiauShop.Areas.Admin.Controllers.ContaController.PoliticaVisitante, contexto =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Avaliações de produto: 3 a cada 10 minutos por IP (evita enxurrada de spam na fila de moderação).
    opcoes.AddPolicy(MiauShop.Controllers.ProdutosController.PoliticaAvaliacao, contexto =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(10)
            }));

    // Assistente da loja: 20 mensagens por minuto por IP (conversa normal fica bem abaixo disso).
    opcoes.AddPolicy(MiauShop.Controllers.AssistenteController.Politica, contexto =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            }));

    opcoes.OnRejected = (contexto, _) =>
    {
        var http = contexto.HttpContext;

        // O chat chama pelo JavaScript: responde 429 (o chat mostra "aguarde um minuto") em vez de redirecionar.
        if (http.Request.Path.StartsWithSegments("/Assistente"))
        {
            http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return ValueTask.CompletedTask;
        }

        var destino = http.Request.Path.StartsWithSegments("/Admin")
            ? "/Admin/Conta/Entrar?bloqueado=true"
            : http.GetRouteValue("slug") is string slug
                // Avaliação de produto: volta para a página do produto, com o aviso.
                ? $"/Produtos/{Uri.EscapeDataString(slug)}?bloqueado=true#avaliacoes"
                : "/Pedido/Acompanhar?bloqueado=true";
        contexto.HttpContext.Response.Redirect(destino);
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

// Cria/atualiza o banco e insere os dados de exemplo (só na primeira vez).
using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InicializarAsync(contexto);
    await SeedDetalhesProdutos.InicializarAsync(contexto);
    await SeedAssistente.InicializarAsync(contexto);

    // Papéis do painel e usuários iniciais (administrador da configuração e, se ligado, o de demonstração).
    await SeedUsuarios.InicializarAsync(
        escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>(),
        escopo.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        escopo.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpcoesAdministrador>>().Value,
        app.Logger);
}

// Primeiro de tudo: entender os cabeçalhos do proxy do Render (HTTPS e IP real do visitante).
app.UseForwardedHeaders();

// Cabeçalhos de segurança em todas as respostas.
app.Use(async (contexto, proximo) =>
{
    var cabecalhos = contexto.Response.Headers;
    cabecalhos["X-Content-Type-Options"] = "nosniff";                 // o navegador não "adivinha" o tipo do arquivo
    cabecalhos["X-Frame-Options"] = "DENY";                           // o site não pode ser embutido em outro (clickjacking)
    cabecalhos["Referrer-Policy"] = "strict-origin-when-cross-origin";
    cabecalhos["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

    // CSP: de onde a página pode carregar scripts, estilos, fontes e imagens.
    // Scripts só deste site e das CDNs usadas (sem script inline); o Mercado Pago fica liberado para quando for ligado.
    cabecalhos["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' https://cdnjs.cloudflare.com https://sdk.mercadopago.com https://*.mercadopago.com https://*.mlstatic.com; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://*.mercadopago.com https://*.mercadolibre.com; " +
        "frame-src https://*.mercadopago.com https://*.mercadolibre.com; " +
        "frame-ancestors 'none'; form-action 'self'; base-uri 'self'";

    await proximo();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Português do Brasil: R$ 54,90 em vez de $54.90, datas dd/mm/aaaa.
app.UseRequestLocalization("pt-BR");

app.UseRouting();
app.UseRateLimiter();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/saude");

// Painel: /Admin/{controller}/{action} (área "Admin").
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
