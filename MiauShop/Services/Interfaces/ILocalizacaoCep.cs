using MiauShop.Models;

namespace MiauShop.Services.Interfaces;

public interface ILocalizacaoCep
{
    // Cidade, UF e coordenada do CEP, ou null se o CEP não existir.
    Task<LocalCep?> LocalizarAsync(string cep);
}
