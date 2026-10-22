namespace MiauShop.Models.ViewModels;

public class CheckoutViewModel
{
    public required DadosCheckout Dados { get; init; }
    public required Carrinho Carrinho { get; init; }

    // Preenchida só na etapa de pagamento quando o Mercado Pago está configurado.
    public string? PublicKeyMercadoPago { get; init; }
}
