using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.Property(c => c.Nome).HasMaxLength(80).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Descricao).HasMaxLength(300);
        builder.Property(c => c.Icone).HasMaxLength(60).IsRequired();

        builder.HasIndex(c => c.Slug).IsUnique();
    }
}
