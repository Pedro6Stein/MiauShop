using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Models.ValueObjects;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

public class CheckoutService(
    ICarrinhoService carrinhoService,
    IPedidoRepositorio pedidoRepositorio) : ICheckoutService
{
    public async Task<int> FinalizarAsync(
        string clienteNome,
        string clienteEmail,
        string clienteTelefone,
        Endereco enderecoEntrega,
        FormaPagamento formaPagamento)
    {
        // Preços e frete vêm do carrinho montado com dados do banco, nunca do formulário.
        var carrinho = await carrinhoService.ObterAsync();

        if (carrinho.Vazio)
            throw new InvalidOperationException("Seu carrinho está vazio.");

        if (carrinho.ItensForamRemovidos)
            throw new InvalidOperationException("Alguns itens ficaram indisponíveis. Confira o carrinho antes de finalizar.");

        var pedido = Pedido.Criar(clienteNome, clienteEmail, clienteTelefone, enderecoEntrega, formaPagamento);

        foreach (var linha in carrinho.Linhas)
        {
            var imagem = linha.Variacao.Produto.ImagemPrincipalUrl();
            pedido.AdicionarItem(ItemPedido.CriarDe(linha.Variacao, linha.Quantidade, imagem));
        }

        pedido.DefinirFrete(carrinho.Frete);

        // Pedido, itens e histórico são gravados juntos em um único SaveChanges (uma transação).
        pedidoRepositorio.Adicionar(pedido);
        await pedidoRepositorio.SalvarAsync();

        carrinhoService.Limpar();

        return pedido.Id;
    }
}
