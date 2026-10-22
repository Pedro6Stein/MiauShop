using MiauShop.Models.Enums;
using MiauShop.Services;

namespace MiauShop.Tests;

public class MapeamentoPagamentoTests
{
    [Theory]
    [InlineData("bank_transfer", FormaPagamento.Pix)]
    [InlineData("credit_card", FormaPagamento.Cartao)]
    [InlineData("debit_card", FormaPagamento.Cartao)]
    [InlineData("ticket", FormaPagamento.Boleto)]
    public void MetodosDoMercadoPago_ViramNossaFormaDePagamento(string metodo, FormaPagamento esperado)
    {
        Assert.Equal(esperado, MapeamentoPagamento.DoMercadoPago(metodo));
    }

    [Theory]
    [InlineData("bitcoin")]
    [InlineData("")]
    [InlineData(null)]
    public void MetodoDesconhecido_ERecusado(string? metodo)
    {
        Assert.Null(MapeamentoPagamento.DoMercadoPago(metodo));
    }
}
