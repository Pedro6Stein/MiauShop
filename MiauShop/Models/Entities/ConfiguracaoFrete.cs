namespace MiauShop.Models.Entities;

// Tabela do frete por distância, editada no painel (Admin > Frete).
// Frete = valor base + (km por estrada × valor por km), limitado ao valor máximo.
// O frete grátis acima de um valor de compra continua valendo por cima desta tabela.
public class ConfiguracaoFrete : EntidadeBase
{
    // Centro de Bragança Paulista (Rua Coronel Teófilo Leme): de onde os pedidos saem por padrão.
    public const string CepOrigemPadrao = "12900-002";

    public string CepOrigem { get; set; } = CepOrigemPadrao;
    public string CidadeOrigem { get; set; } = "Bragança Paulista";
    public string UfOrigem { get; set; } = "SP";

    public decimal ValorBase { get; set; } = 12.00m;
    public decimal ValorPorKm { get; set; } = 0.040m;
    public decimal ValorMaximo { get; set; } = 59.90m;

    public static ConfiguracaoFrete Padrao() => new();

    // Ex.: base 12,00 + 90 km × 0,04 = 15,60.
    public decimal ValorPara(int distanciaKm) =>
        Math.Min(ValorMaximo, Math.Round(ValorBase + distanciaKm * ValorPorKm, 2, MidpointRounding.AwayFromZero));

    public IEnumerable<string> ValidarRegras()
    {
        if (ValorBase < 0)
            yield return "O valor base não pode ser negativo.";
        if (ValorPorKm < 0)
            yield return "O valor por km não pode ser negativo.";
        if (ValorMaximo < ValorBase)
            yield return "O valor máximo precisa ser maior ou igual ao valor base.";
    }
}
