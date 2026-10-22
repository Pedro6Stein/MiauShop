namespace MiauShop.Services.Interfaces;

// FaltaParaFreteGratis alimenta a mensagem "Faltam R$ X para frete grátis!".
public record ResultadoFrete(decimal Valor, decimal FaltaParaFreteGratis)
{
    public bool Gratis => Valor == 0;
}

// Interface: hoje existe só o frete fixo; um frete por CEP seria outra classe com o mesmo contrato.
public interface ICalculadoraFrete
{
    ResultadoFrete Calcular(decimal subtotal);
}
