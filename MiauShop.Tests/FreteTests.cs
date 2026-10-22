using MiauShop.Services;

namespace MiauShop.Tests;

public class FreteTests
{
    private readonly FreteFixoComGratis _frete = new();

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
