using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.Property(p => p.ClienteNome).HasMaxLength(120).IsRequired();
        builder.Property(p => p.ClienteEmail).HasMaxLength(160).IsRequired();
        builder.Property(p => p.ClienteTelefone).HasMaxLength(20).IsRequired();

        builder.HasIndex(p => p.ClienteEmail);
        builder.HasIndex(p => p.Status);

        // Endereço vira colunas dentro da própria tabela Pedidos (Entrega_Cep, Entrega_Cidade...).
        builder.ComplexProperty(p => p.EnderecoEntrega, endereco =>
        {
            endereco.Property(e => e.Cep).HasColumnName("Entrega_Cep").HasMaxLength(9).IsRequired();
            endereco.Property(e => e.Logradouro).HasColumnName("Entrega_Logradouro").HasMaxLength(150).IsRequired();
            endereco.Property(e => e.Numero).HasColumnName("Entrega_Numero").HasMaxLength(20).IsRequired();
            endereco.Property(e => e.Complemento).HasColumnName("Entrega_Complemento").HasMaxLength(80);
            endereco.Property(e => e.Bairro).HasColumnName("Entrega_Bairro").HasMaxLength(80).IsRequired();
            endereco.Property(e => e.Cidade).HasColumnName("Entrega_Cidade").HasMaxLength(80).IsRequired();
            endereco.Property(e => e.Uf).HasColumnName("Entrega_Uf").HasMaxLength(2).IsRequired();
        });

        // Visitante não tem usuário; se um dia o usuário for apagado, o pedido fica.
        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        // Itens e histórico são lidos/gravados pelas listas privadas (_itens, _historico).
        builder.HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Historico)
            .WithOne(h => h.Pedido)
            .HasForeignKey(h => h.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Historico).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
