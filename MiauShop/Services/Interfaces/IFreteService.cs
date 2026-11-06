using MiauShop.Models;
using MiauShop.Models.Entities;

namespace MiauShop.Services.Interfaces;

// Frete por distância entre o CEP de origem (configurado no painel) e o CEP do cliente.
public interface IFreteService
{
    decimal MinimoParaFreteGratis { get; }

    // null = CEP não encontrado.
    Task<CotacaoFrete?> CotarAsync(string cep, decimal subtotal);

    Task<ConfiguracaoFrete> ObterConfiguracaoAsync();

    // Valida o CEP de origem, completa cidade/UF e salva. Devolve os erros (vazio = salvo).
    Task<List<string>> SalvarConfiguracaoAsync(string cepOrigem, decimal valorBase, decimal valorPorKm, decimal valorMaximo);
}
