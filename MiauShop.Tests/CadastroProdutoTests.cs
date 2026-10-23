using MiauShop.Models.Entities;
using MiauShop.Services;

namespace MiauShop.Tests;

public class CadastroProdutoTests
{
    [Theory]
    [InlineData("Comidinhas & Petiscos", "comidinhas-e-petiscos")]
    [InlineData("Ração Natural Filhotes", "racao-natural-filhotes")]
    [InlineData("  Sachê Gourmet (caixa c/ 12) ", "sache-gourmet-caixa-c-12")]
    public void Slug_SemAcentosEspacosOuSimbolos(string nome, string esperado)
    {
        Assert.Equal(esperado, GeradorSlug.Gerar(nome));
    }

    [Fact]
    public void Slug_RepetidoGanhaNumero()
    {
        Assert.Equal("bandana-xadrez-3", GeradorSlug.Unico("Bandana Xadrez", ["bandana-xadrez", "bandana-xadrez-2"]));
    }

    [Fact]
    public void PromocaoMaiorQueOPreco_ERecusada()
    {
        var produto = new Produto { Preco = 50m, PrecoPromocional = 60m, Ativo = false };

        Assert.Contains(produto.ValidarRegras(), erro => erro.Contains("promocional"));
    }

    [Fact]
    public void ProdutoAtivoSemVariacaoAtiva_ERecusado()
    {
        var produto = new Produto { Slug = "bolinha", Preco = 20m, Ativo = true };
        produto.AdicionarVariacao("Único", 0).Ativa = false;

        Assert.Contains(produto.ValidarRegras(), erro => erro.Contains("variação ativa"));
    }

    [Fact]
    public void ProdutoValido_NaoTemErros_ESkuSegueOSlug()
    {
        var produto = new Produto { Slug = "moletom-pet", Preco = 70m, PrecoPromocional = 55m, Ativo = true };
        produto.AdicionarVariacao("P", 0);
        var tamanhoG = produto.AdicionarVariacao("G", 5m);

        Assert.Empty(produto.ValidarRegras());
        Assert.Equal("moletom-pet-2", tamanhoG.Sku);
        Assert.Equal(60m, tamanhoG.PrecoFinal());
    }
}
