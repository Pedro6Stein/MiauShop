using MiauShop.Models.Entities;

namespace MiauShop.Tests;

public class ProdutoTests
{
    [Fact]
    public void Promocao_UsaPrecoPromocionalECalculaDesconto()
    {
        var produto = new Produto { Preco = 50m, PrecoPromocional = 39m };

        Assert.True(produto.EmPromocao());
        Assert.Equal(39m, produto.PrecoAtual());
        Assert.Equal(22, produto.PercentualDesconto());
    }

    [Fact]
    public void PrecoPromocionalMaiorQueOPreco_NaoContaComoPromocao()
    {
        var produto = new Produto { Preco = 50m, PrecoPromocional = 60m };

        Assert.False(produto.EmPromocao());
        Assert.Equal(50m, produto.PrecoAtual());
    }

    [Fact]
    public void Variacao_SomaAjusteAoPrecoAtualDoProduto()
    {
        var produto = new Produto { Preco = 80m, PrecoPromocional = 70m };
        var tamanhoG = new ProdutoVariacao { Produto = produto, AjustePreco = 5m };

        Assert.Equal(75m, tamanhoG.PrecoFinal());
    }
}
