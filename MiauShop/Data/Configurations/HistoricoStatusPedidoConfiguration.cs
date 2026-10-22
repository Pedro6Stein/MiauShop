using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class HistoricoStatusPedidoConfiguration : IEntityTypeConfiguration<HistoricoStatusPedido>
{
    public void Configure(EntityTypeBuilder<HistoricoStatusPedido> builder)
    {
        builder.ToTable("HistoricoStatusPedidos");

        builder.Property(h => h.Observacao).HasMaxLength(300);
    }
}
