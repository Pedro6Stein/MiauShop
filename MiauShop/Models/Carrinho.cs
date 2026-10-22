using MiauShop.Models.Entities;

namespace MiauShop.Models;

// Carrinho já montado com os preços do banco (não é salvo no banco; vem da sessão).
public class Carrinho
{
    public List<LinhaCarrinho> Linhas { get; } = new();

    public decimal Frete { get; set; }
    public decimal FaltaParaFreteGratis { get; set; }

    // true quando algum item da sessão foi desativado e saiu do carrinho.
    public bool ItensForamRemovidos { get; set; }

    public bool Vazio => Linhas.Count == 0;
    public int QuantidadeItens => Linhas.Sum(l => l.Quantidade);
    public decimal Subtotal => Linhas.Sum(l => l.Subtotal);
    public decimal Total => Subtotal + Frete;

    // Quanto da barra de frete grátis já está cheio (0 a 100).
    public int PercentualFreteGratis =>
        FaltaParaFreteGratis <= 0 ? 100 : (int)(Subtotal / (Subtotal + FaltaParaFreteGratis) * 100);
}

public record LinhaCarrinho(ProdutoVariacao Variacao, int Quantidade)
{
    public decimal PrecoUnitario => Variacao.PrecoFinal();
    public decimal Subtotal => PrecoUnitario * Quantidade;
}
