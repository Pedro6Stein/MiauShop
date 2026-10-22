using MiauShop.Models.Enums;
using MiauShop.Models.ValueObjects;

namespace MiauShop.Services.Interfaces;

public interface ICheckoutService
{
    // Cria o pedido a partir do carrinho e devolve o número (Id) do pedido.
    Task<int> FinalizarAsync(
        string clienteNome,
        string clienteEmail,
        string clienteTelefone,
        Endereco enderecoEntrega,
        FormaPagamento formaPagamento);
}
