using System.Text.Json.Serialization;
using MiauShop.Models.ValueObjects;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

// Consulta o ViaCEP (gratuito, sem chave) pelo servidor; o navegador só conversa com o MiauShop.
public class ViaCepService(HttpClient http, ILogger<ViaCepService> logger) : IConsultaCep
{
    private record RespostaViaCep(
        [property: JsonPropertyName("cep")] string? Cep,
        [property: JsonPropertyName("logradouro")] string? Logradouro,
        [property: JsonPropertyName("bairro")] string? Bairro,
        [property: JsonPropertyName("localidade")] string? Localidade,
        [property: JsonPropertyName("uf")] string? Uf,
        [property: JsonPropertyName("erro")] object? Erro);

    public async Task<Endereco?> ConsultarAsync(string cep)
    {
        var digitos = new string(cep.Where(char.IsDigit).ToArray());
        if (digitos.Length != 8)
            return null;

        try
        {
            var resposta = await http.GetFromJsonAsync<RespostaViaCep>($"{digitos}/json/");

            if (resposta is null || resposta.Erro is not null)
                return null;

            return new Endereco
            {
                Cep = resposta.Cep ?? digitos,
                Logradouro = resposta.Logradouro ?? string.Empty,
                Bairro = resposta.Bairro ?? string.Empty,
                Cidade = resposta.Localidade ?? string.Empty,
                Uf = resposta.Uf ?? string.Empty
            };
        }
        catch (Exception erro) when (erro is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            // Se o ViaCEP estiver fora do ar, o cliente só preenche o endereço à mão.
            logger.LogWarning(erro, "Falha ao consultar o CEP {Cep}", digitos);
            return null;
        }
    }
}
