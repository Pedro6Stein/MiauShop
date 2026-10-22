namespace MiauShop.Models.Entities;

public class ProdutoImagem : EntidadeBase
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    // Hoje: "/img/produtos/arquivo.webp". Futuro: URL do Cloudinary.
    public string Url { get; set; } = string.Empty;

    // Descrição da imagem para leitores de tela (acessibilidade).
    public string TextoAlternativo { get; set; } = string.Empty;

    public int Ordem { get; set; }

    // Imagem usada no card da vitrine.
    public bool Principal { get; set; }

    // Id do arquivo no serviço externo (Cloudinary). Vazio enquanto as imagens forem locais.
    public string? ArmazenamentoId { get; set; }
}
