using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class ProdutoVariacaoConfiguration : IEntityTypeConfiguration<ProdutoVariacao>
{
    public void Configure(EntityTypeBuilder<ProdutoVariacao> builder)
    {
        builder.ToTable("ProdutoVariacoes");

        builder.Property(v => v.Nome).HasMaxLength(60).IsRequired();
        builder.Property(v => v.Sku).HasMaxLength(40).IsRequired();

        builder.HasIndex(v => v.Sku).IsUnique();
    }
}
