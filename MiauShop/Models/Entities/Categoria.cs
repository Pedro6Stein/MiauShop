namespace MiauShop.Models.Entities;

public class Categoria : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;

    // Versão do nome usada na URL: "Comidinhas & Petiscos" -> "comidinhas-e-petiscos"
    public string Slug { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    // Nome do ícone exibido no menu (ex.: "bi-balloon-heart")
    public string Icone { get; set; } = string.Empty;

    public int Ordem { get; set; }

    // Exclusão lógica: categoria desativada some da loja, mas continua no banco.
    public bool Ativa { get; set; } = true;

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
