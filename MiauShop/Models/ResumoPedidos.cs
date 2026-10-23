using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Models;

public record ProdutoMaisVendido(int ProdutoId, string Nome, int Quantidade, decimal Receita);

// Números do dashboard do painel, calculados no banco.
public record ResumoPedidos(
    Dictionary<StatusPedido, int> PorStatus,
    decimal Faturamento,
    List<ProdutoMaisVendido> MaisVendidos,
    List<Pedido> Ultimos);
