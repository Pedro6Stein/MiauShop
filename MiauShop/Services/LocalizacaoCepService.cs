using System.Globalization;
using System.Text.Json.Serialization;
using MiauShop.Models;
using MiauShop.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MiauShop.Services;

// Coordenada do CEP pela AwesomeAPI (gratuita, sem chave). Se ela falhar, usa o ViaCEP para saber
// o estado e pega a coordenada da capital (frete aproximado, mas a compra continua).
// Cada CEP fica 24 h em memória: o mesmo CEP não é consultado de novo a cada página.
public class LocalizacaoCepService(
    HttpClient http,
    IConsultaCep consultaCep,
    IMemoryCache cache,
    ILogger<LocalizacaoCepService> logger) : ILocalizacaoCep
{
    private record RespostaAwesome(
        [property: JsonPropertyName("cep")] string? Cep,
        [property: JsonPropertyName("city")] string? Cidade,
        [property: JsonPropertyName("state")] string? Uf,
        [property: JsonPropertyName("lat")] string? Latitude,
        [property: JsonPropertyName("lng")] string? Longitude);

    public async Task<LocalCep?> LocalizarAsync(string cep)
    {
        var digitos = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length != 8)
            return null;

        var chave = $"cep-local:{digitos}";
        if (cache.TryGetValue(chave, out LocalCep? salvo))
            return salvo;

        var local = await PelaAwesomeApiAsync(digitos) ?? await PelaCapitalDoEstadoAsync(digitos);
        if (local is not null)
            cache.Set(chave, local, TimeSpan.FromHours(24));

        return local;
    }

    private async Task<LocalCep?> PelaAwesomeApiAsync(string digitos)
    {
        try
        {
            var resposta = await http.GetFromJsonAsync<RespostaAwesome>(digitos);
            if (resposta is null
                || !double.TryParse(resposta.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                || !double.TryParse(resposta.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
                return null;

            return new LocalCep(Formatar(digitos), resposta.Cidade ?? string.Empty, resposta.Uf ?? string.Empty,
                new Coordenada(lat, lng), Aproximado: false);
        }
        catch (Exception erro) when (erro is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            // 404 (CEP não encontrado) também cai aqui; o ViaCEP confirma se o CEP existe.
            logger.LogInformation("AwesomeAPI sem coordenada para {Cep}: {Erro}", digitos, erro.Message);
            return null;
        }
    }

    private async Task<LocalCep?> PelaCapitalDoEstadoAsync(string digitos)
    {
        var endereco = await consultaCep.ConsultarAsync(digitos);
        var capital = Coordenada.CapitalDe(endereco?.Uf);

        return endereco is null || capital is null
            ? null
            : new LocalCep(Formatar(digitos), endereco.Cidade, endereco.Uf, capital, Aproximado: true);
    }

    private static string Formatar(string digitos) => $"{digitos[..5]}-{digitos[5..]}";
}
