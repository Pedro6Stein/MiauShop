using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class AvaliacaoProdutoConfiguration : IEntityTypeConfiguration<AvaliacaoProduto>
{
    public void Configure(EntityTypeBuilder<AvaliacaoProduto> builder)
    {
        builder.ToTable("AvaliacoesProduto", tabela =>
            tabela.HasCheckConstraint("CK_AvaliacoesProduto_Nota",
                $"\"Nota\" BETWEEN {AvaliacaoProduto.NotaMinima} AND {AvaliacaoProduto.NotaMaxima}"));

        builder.Property(a => a.Nome).HasMaxLength(AvaliacaoProduto.TamanhoMaximoNome).IsRequired();
        builder.Property(a => a.Comentario).HasMaxLength(AvaliacaoProduto.TamanhoMaximoComentario).IsRequired();

        // A loja lê "avaliações aprovadas deste produto"; o painel lista as pendentes.
        builder.HasIndex(a => new { a.ProdutoId, a.Aprovada });

        // Avaliação pertence ao produto (produto vendido nunca é apagado, só desativado).
        builder.HasOne(a => a.Produto)
            .WithMany(p => p.Avaliacoes)
            .HasForeignKey(a => a.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
