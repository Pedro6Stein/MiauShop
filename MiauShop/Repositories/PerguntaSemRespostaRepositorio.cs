using MiauShop.Data;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Repositories;

public class PerguntaSemRespostaRepositorio(AppDbContext contexto)
    : Repositorio<PerguntaSemResposta>(contexto), IPerguntaSemRespostaRepositorio
{
    public Task<PerguntaSemResposta?> ObterPorTextoAsync(string textoNormalizado) =>
        Conjunto.FirstOrDefaultAsync(p => p.TextoNormalizado == textoNormalizado);

    public Task<List<PerguntaSemResposta>> ListarMaisFrequentesAsync(int quantidade) =>
        Conjunto.AsNoTracking()
            .OrderByDescending(p => p.Vezes)
            .ThenByDescending(p => p.AtualizadoEm)
            .Take(quantidade)
            .ToListAsync();
}
