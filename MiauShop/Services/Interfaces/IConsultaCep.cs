using MiauShop.Models.ValueObjects;

namespace MiauShop.Services.Interfaces;

public interface IConsultaCep
{
    // Devolve rua, bairro, cidade e UF do CEP, ou null se não existir ou o serviço falhar.
    Task<Endereco?> ConsultarAsync(string cep);
}
