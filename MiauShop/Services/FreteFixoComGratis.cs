using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class FreteFixoComGratis : ICalculadoraFrete
{
    public const decimal ValorFixo = 19.90m;
    public const decimal MinimoParaFreteGratis = 150m;

    public ResultadoFrete Calcular(decimal subtotal) =>
        subtotal >= MinimoParaFreteGratis
            ? new ResultadoFrete(0, 0)
            : new ResultadoFrete(ValorFixo, MinimoParaFreteGratis - subtotal);
}
