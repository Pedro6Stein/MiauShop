using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Models.ViewModels;

public class CatalogoViewModel
{
    public required FiltroProdutos Filtro { get; init; }
    public required List<Produto> Produtos { get; init; }
    public required List<Categoria> Categorias { get; init; }
    public int Total { get; init; }
    public int ItensPorPagina { get; init; }

    public int TotalPaginas => (int)Math.Ceiling(Total / (double)ItensPorPagina);

    // Faixas de preço oferecidas no filtro (mínimo, máximo).
    public static readonly (string Rotulo, int? Minimo, int? Maximo)[] FaixasDePreco =
    [
        ("Até R$ 30", null, 30),
        ("R$ 30 a R$ 80", 30, 80),
        ("R$ 80 a R$ 150", 80, 150),
        ("Acima de R$ 150", 150, null)
    ];

    public string Titulo =>
        !string.IsNullOrWhiteSpace(Filtro.Busca) ? $"Resultados para \"{Filtro.Busca}\""
        : Categorias.FirstOrDefault(c => c.Slug == Filtro.Categoria)?.Nome
        ?? (Filtro.Animal is Animal animal ? $"Para {animal.NomeExibicao().ToLower()}" : "Todos os produtos");
}
