using MiauShop.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MiauShop.Services;

public class FreteFixoComGratis(IOptions<OpcoesFrete> opcoes) : ICalculadoraFrete
{
    private readonly OpcoesFrete _opcoes = opcoes.Value;

    public decimal ValorFixo => _opcoes.ValorFixo;
    public decimal MinimoParaFreteGratis => _opcoes.MinimoParaFreteGratis;

    public ResultadoFrete Calcular(decimal subtotal) =>
        subtotal >= MinimoParaFreteGratis
            ? new ResultadoFrete(0, 0)
            : new ResultadoFrete(ValorFixo, MinimoParaFreteGratis - subtotal);
}
