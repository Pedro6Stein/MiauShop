using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.Property(i => i.NomeProduto).HasMaxLength(120).IsRequired();
        builder.Property(i => i.NomeVariacao).HasMaxLength(60).IsRequired();
        builder.Property(i => i.ImagemUrl).HasMaxLength(500).IsRequired();

        // Restrict: uma variação que já foi vendida não pode ser apagada.
        builder.HasOne(i => i.ProdutoVariacao)
            .WithMany()
            .HasForeignKey(i => i.ProdutoVariacaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
