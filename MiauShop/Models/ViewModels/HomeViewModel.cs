using MiauShop.Models.Entities;

namespace MiauShop.Models.ViewModels;

public class HomeViewModel
{
    public List<Categoria> Categorias { get; init; } = [];
    public List<Produto> Destaques { get; init; } = [];
}
