using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class ConfiguracaoFreteConfiguration : IEntityTypeConfiguration<ConfiguracaoFrete>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoFrete> builder)
    {
        builder.ToTable("ConfiguracoesFrete");

        builder.Property(c => c.CepOrigem).HasMaxLength(9).IsRequired();
        builder.Property(c => c.CidadeOrigem).HasMaxLength(80).IsRequired();
        builder.Property(c => c.UfOrigem).HasMaxLength(2).IsRequired();

        // Centavos de centavo no valor por km (ex.: 0,035).
        builder.Property(c => c.ValorPorKm).HasPrecision(10, 3);
    }
}
