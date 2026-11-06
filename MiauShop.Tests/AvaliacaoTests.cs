using MiauShop.Models;
using MiauShop.Models.Entities;

namespace MiauShop.Tests;

public class AvaliacaoTests
{
    [Fact]
    public void Resumo_CalculaMediaPercentuaisEMeiaEstrela()
    {
        // notas: 5, 5, 5, 4, 2  -> média 4,2
        var resumo = new ResumoAvaliacoes([0, 1, 0, 1, 3], []);

        Assert.Equal(5, resumo.Total);
        Assert.Equal(4.2, resumo.Media, precision: 2);
        Assert.Equal(4.0, resumo.MediaEmMeiaEstrela);
        Assert.Equal(60, resumo.Percentual(5));
        Assert.Equal(0, resumo.Percentual(1));
    }

    [Fact]
    public void Resumo_ArredondaParaMeiaEstrela()
    {
        // notas: 5, 4  -> média 4,5
        var resumo = new ResumoAvaliacoes([0, 0, 0, 1, 1], []);
        Assert.Equal(4.5, resumo.MediaEmMeiaEstrela);
    }

    [Fact]
    public void Resumo_SemAvaliacoes_NaoDividePorZero()
    {
        var resumo = ResumoAvaliacoes.Vazio;

        Assert.Equal(0, resumo.Total);
        Assert.Equal(0, resumo.Media);
        Assert.Equal(0, resumo.Percentual(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Nova_NotaForaDeUmACinco_EhRecusada(int nota)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AvaliacaoProduto.Nova(1, "Ana", nota, "Muito bom mesmo"));
    }

    [Fact]
    public void Nova_EntraPendenteEComTextoLimpo()
    {
        var avaliacao = AvaliacaoProduto.Nova(7, "  Ana Souza ", 5, "  Adorei!  ");

        Assert.False(avaliacao.Aprovada);
        Assert.Equal("Ana Souza", avaliacao.Nome);
        Assert.Equal("Adorei!", avaliacao.Comentario);

        avaliacao.Aprovar();
        Assert.True(avaliacao.Aprovada);
    }

    [Theory]
    [InlineData("Ana Maria Souza", "Ana S.")]
    [InlineData("Ana", "Ana")]
    [InlineData("   ", "Cliente")]
    public void NomeExibicao_MostraPrimeiroNomeEInicial(string nome, string esperado)
    {
        var avaliacao = AvaliacaoProduto.Nova(1, nome, 4, "Comentário qualquer");
        Assert.Equal(esperado, avaliacao.NomeExibicao());
    }

    [Fact]
    public void Produto_ListaCaracteristicas_IgnoraLinhasVaziasEMarcadores()
    {
        var produto = new Produto { Caracteristicas = "- Lavável na máquina\r\n\r\n• Não solta pelos\n  Leve  \n" };

        Assert.Equal(["Lavável na máquina", "Não solta pelos", "Leve"], produto.ListaCaracteristicas());
    }

    [Fact]
    public void Produto_TemFichaTecnica_SoQuandoAlgumCampoPreenchido()
    {
        Assert.False(new Produto().TemFichaTecnica());
        Assert.False(new Produto { Material = "  " }.TemFichaTecnica());
        Assert.True(new Produto { Garantia = "90 dias" }.TemFichaTecnica());
    }
}
