using MiauShop.Data;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Repositories;

public class PedidoRepositorio(AppDbContext contexto) : Repositorio<Pedido>(contexto), IPedidoRepositorio
{
    public Task<Pedido?> ObterCompletoAsync(int id) =>
        Conjunto
            .Include(p => p.Itens)
            .Include(p => p.Historico)
            .FirstOrDefaultAsync(p => p.Id == id);
}
