using MiauShop.Services;
using Microsoft.Extensions.Options;

namespace MiauShop.Tests;

public class FreteTests
{
    private readonly FreteFixoComGratis _frete = new(Options.Create(new OpcoesFrete
    {
        ValorFixo = 19.90m,
        MinimoParaFreteGratis = 150m
    }));

    [Fact]
    public void AbaixoDoMinimo_CobraFreteFixoEInformaQuantoFalta()
    {
        var resultado = _frete.Calcular(100m);

        Assert.Equal(19.90m, resultado.Valor);
        Assert.Equal(50m, resultado.FaltaParaFreteGratis);
    }

    [Fact]
    public void APartirDoMinimo_FreteGratis()
    {
        Assert.True(_frete.Calcular(150m).Gratis);
    }
}
