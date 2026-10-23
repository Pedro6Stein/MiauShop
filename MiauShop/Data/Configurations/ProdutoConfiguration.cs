using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        // uint + IsRowVersion = coluna de sistema "xmin" do PostgreSQL (controle de concorrência).
        builder.Property(p => p.Versao).IsRowVersion();

        builder.Property(p => p.Nome).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(140).IsRequired();
        builder.Property(p => p.DescricaoCurta).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(4000).IsRequired();

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => new { p.Ativo, p.Animal });

        // Restrict: não deixa apagar categoria/fornecedor que ainda tem produtos.
        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Fornecedor)
            .WithMany(f => f.Produtos)
            .HasForeignKey(p => p.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: variações e imagens pertencem ao produto.
        builder.HasMany(p => p.Variacoes)
            .WithOne(v => v.Produto)
            .HasForeignKey(v => v.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Imagens)
            .WithOne(i => i.Produto)
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
