namespace MiauShop.Services;

// Regras do frete, lidas da seção "Frete" do appsettings.json.
// Mudar o valor do frete não exige mexer em código nem nas telas.
public class OpcoesFrete
{
    public const string Secao = "Frete";

    public decimal ValorFixo { get; set; }
    public decimal MinimoParaFreteGratis { get; set; }
}
