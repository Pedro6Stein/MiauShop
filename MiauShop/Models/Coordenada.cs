namespace MiauShop.Models;

// Latitude e longitude de um CEP (em graus).
public record Coordenada(double Latitude, double Longitude)
{
    // Plano B quando o serviço de coordenadas está fora do ar: a capital do estado do CEP
    // (o estado vem do ViaCEP). O frete fica aproximado, mas a compra não trava.
    private static readonly Dictionary<string, Coordenada> Capitais = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AC"] = new(-9.975, -67.810), ["AL"] = new(-9.666, -35.735), ["AM"] = new(-3.119, -60.022),
        ["AP"] = new(0.035, -51.070), ["BA"] = new(-12.971, -38.501), ["CE"] = new(-3.732, -38.527),
        ["DF"] = new(-15.794, -47.882), ["ES"] = new(-20.315, -40.312), ["GO"] = new(-16.686, -49.264),
        ["MA"] = new(-2.530, -44.303), ["MG"] = new(-19.917, -43.935), ["MS"] = new(-20.469, -54.620),
        ["MT"] = new(-15.601, -56.097), ["PA"] = new(-1.456, -48.490), ["PB"] = new(-7.115, -34.864),
        ["PE"] = new(-8.048, -34.877), ["PI"] = new(-5.089, -42.802), ["PR"] = new(-25.429, -49.271),
        ["RJ"] = new(-22.907, -43.173), ["RN"] = new(-5.795, -35.209), ["RO"] = new(-8.761, -63.900),
        ["RR"] = new(2.820, -60.672), ["RS"] = new(-30.035, -51.218), ["SC"] = new(-27.595, -48.548),
        ["SE"] = new(-10.947, -37.073), ["SP"] = new(-23.550, -46.633), ["TO"] = new(-10.184, -48.334)
    };

    public static Coordenada? CapitalDe(string? uf) =>
        uf is not null && Capitais.TryGetValue(uf, out var capital) ? capital : null;
}

// Onde fica um CEP: cidade, UF e coordenada. Aproximado = coordenada da capital (plano B).
public record LocalCep(string Cep, string Cidade, string Uf, Coordenada Coordenada, bool Aproximado);

// Resultado do cálculo de frete para um CEP, já com a regra do frete grátis aplicada.
public record CotacaoFrete(
    decimal Valor,              // o que o cliente paga (0 quando é grátis)
    decimal ValorPelaDistancia, // valor da tabela, antes do frete grátis
    int DistanciaKm,
    int DiasTransporte,
    string Cidade,
    string Uf,
    string CidadeOrigem,
    bool Aproximado)
{
    public bool Gratis => Valor == 0;
}
