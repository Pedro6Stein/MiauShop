using MiauShop.Data;
using MiauShop.Repositories;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Banco de dados: a string de conexão vem do appsettings (local) ou de variável de ambiente (produção).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MiauShop")));

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

// Serviços
builder.Services.Configure<OpcoesFrete>(builder.Configuration.GetSection(OpcoesFrete.Secao));
builder.Services.AddSingleton<ICalculadoraFrete, FreteFixoComGratis>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<ICarrinhoService, CarrinhoService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Checkout: Public Key de teste do Mercado Pago (user secrets / variável de ambiente) e consulta de CEP.
builder.Services.Configure<OpcoesMercadoPago>(builder.Configuration.GetSection(OpcoesMercadoPago.Secao));
builder.Services.AddHttpClient<IConsultaCep, ViaCepService>(cliente =>
{
    cliente.BaseAddress = new Uri("https://viacep.com.br/ws/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

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

    opcoes.OnRejected = (contexto, _) =>
    {
        contexto.HttpContext.Response.Redirect("/Pedido/Acompanhar?bloqueado=true");
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

// Cria/atualiza o banco e insere os dados de exemplo (só na primeira vez).
using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InicializarAsync(contexto);
}

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
