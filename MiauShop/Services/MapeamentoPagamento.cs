using MiauShop.Models.Enums;

namespace MiauShop.Services;

// Converte o método escolhido no Payment Brick do Mercado Pago para a nossa FormaPagamento.
// Qualquer valor desconhecido é recusado (null).
public static class MapeamentoPagamento
{
    private static readonly Dictionary<string, FormaPagamento> Metodos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bank_transfer"] = FormaPagamento.Pix,  // Pix no Brick
        ["pix"] = FormaPagamento.Pix,
        ["credit_card"] = FormaPagamento.Cartao,
        ["debit_card"] = FormaPagamento.Cartao,
        ["ticket"] = FormaPagamento.Boleto,      // boleto no Brick
        ["boleto"] = FormaPagamento.Boleto
    };

    public static FormaPagamento? DoMercadoPago(string? metodo) =>
        metodo is not null && Metodos.TryGetValue(metodo, out var forma) ? forma : null;
}
