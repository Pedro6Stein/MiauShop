using MiauShop.Models.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Data;

// IdentityDbContext<Usuario> já inclui as tabelas de login (AspNetUsers, AspNetRoles...).
// IDataProtectionKeyContext: guarda no banco as chaves que protegem o cookie de login e o
// token antifalsificação, para elas sobreviverem aos reinícios do servidor (o disco do Render é apagado).
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Usuario>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ProdutoVariacao> ProdutoVariacoes => Set<ProdutoVariacao>();
    public DbSet<ProdutoImagem> ProdutoImagens => Set<ProdutoImagem>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<HistoricoStatusPedido> HistoricoStatusPedidos => Set<HistoricoStatusPedido>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Lê todas as classes de Data/Configurations (uma por entidade).
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // Regras que valem para o banco inteiro.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Enums salvos como texto ("Gato") e não como número (1).
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(30);

        // Dinheiro: até 99.999.999,99 com 2 casas decimais.
        configurationBuilder.Properties<decimal>().HavePrecision(10, 2);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PreencherDatas();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PreencherDatas();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Polimorfismo: tratamos todas as entidades como EntidadeBase,
    // sem precisar saber se é Produto, Pedido ou Categoria.
    private void PreencherDatas()
    {
        var agora = DateTime.UtcNow;

        foreach (var entrada in ChangeTracker.Entries<EntidadeBase>())
        {
            if (entrada.State == EntityState.Added)
            {
                // Só preenche se ninguém definiu antes (ex.: histórico com horário retroativo).
                if (entrada.Entity.CriadoEm == default)
                    entrada.Entity.CriadoEm = agora;

                entrada.Entity.AtualizadoEm = agora;
            }
            else if (entrada.State == EntityState.Modified)
            {
                entrada.Entity.AtualizadoEm = agora;
            }
        }
    }
}
