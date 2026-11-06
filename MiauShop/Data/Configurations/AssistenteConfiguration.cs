using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiauShop.Data.Configurations;

public class PerguntaFrequenteConfiguration : IEntityTypeConfiguration<PerguntaFrequente>
{
    public void Configure(EntityTypeBuilder<PerguntaFrequente> builder)
    {
        builder.ToTable("PerguntasFrequentes");
        builder.Property(p => p.Pergunta).HasMaxLength(PerguntaFrequente.TamanhoMaximoPergunta).IsRequired();
        builder.Property(p => p.Resposta).HasMaxLength(PerguntaFrequente.TamanhoMaximoResposta).IsRequired();
        builder.Property(p => p.PalavrasChave).HasMaxLength(PerguntaFrequente.TamanhoMaximoPalavrasChave).IsRequired();
    }
}

public class PerguntaSemRespostaConfiguration : IEntityTypeConfiguration<PerguntaSemResposta>
{
    public void Configure(EntityTypeBuilder<PerguntaSemResposta> builder)
    {
        builder.ToTable("PerguntasSemResposta");
        builder.Property(p => p.Texto).HasMaxLength(PerguntaSemResposta.TamanhoMaximoTexto).IsRequired();
        builder.Property(p => p.TextoNormalizado).HasMaxLength(PerguntaSemResposta.TamanhoMaximoTexto).IsRequired();

        // A mesma dúvida (sem acentos/maiúsculas) vira uma linha só, com a contagem de vezes.
        builder.HasIndex(p => p.TextoNormalizado).IsUnique();
    }
}
