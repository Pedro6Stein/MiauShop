using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class ProdutoImagemConfiguration : IEntityTypeConfiguration<ProdutoImagem>
{
    public void Configure(EntityTypeBuilder<ProdutoImagem> builder)
    {
        builder.ToTable("ProdutoImagens");

        builder.Property(i => i.Url).HasMaxLength(500).IsRequired();
        builder.Property(i => i.TextoAlternativo).HasMaxLength(200).IsRequired();
        builder.Property(i => i.ArmazenamentoId).HasMaxLength(200);
    }
}
