using System.ComponentModel.DataAnnotations;
using MiauShop.Models.Enums;

namespace MiauShop.Models;

public enum OrdenacaoProdutos
{
    [Display(Name = "Mais relevantes")]
    Relevancia,

    [Display(Name = "Menor preço")]
    MenorPreco,

    [Display(Name = "Maior preço")]
    MaiorPreco,

    [Display(Name = "Maior desconto")]
    MaiorDesconto,

    [Display(Name = "Novidades")]
    Novidades,

    [Display(Name = "Nome (A–Z)")]
    NomeAZ
}

// Filtros da listagem de produtos. Vem direto da URL:
// /Produtos?animal=Gato&categoria=brinquedos&promocao=true&precoMax=80&ordem=MenorPreco&pagina=2
public class FiltroProdutos
{
    public Animal? Animal { get; set; }
    public string? Categoria { get; set; }
    public string? Busca { get; set; }
    public bool Promocao { get; set; }
    public int? PrecoMin { get; set; }
    public int? PrecoMax { get; set; }
    public OrdenacaoProdutos Ordem { get; set; }
    public int Pagina { get; set; } = 1;

    public bool TemFiltroAtivo =>
        Animal is not null || Categoria is not null || !string.IsNullOrWhiteSpace(Busca)
        || Promocao || PrecoMin is not null || PrecoMax is not null;

    // Cria uma cópia com uma alteração e volta para a página 1 (usado nos links dos filtros).
    public FiltroProdutos Com(Action<FiltroProdutos> alterar)
    {
        var copia = (FiltroProdutos)MemberwiseClone();
        copia.Pagina = 1;
        alterar(copia);
        return copia;
    }

    // Monta a URL só com os filtros preenchidos.
    public string ParaUrl()
    {
        var partes = new List<string>();

        if (Animal is not null) partes.Add($"animal={Animal}");
        if (!string.IsNullOrWhiteSpace(Categoria)) partes.Add($"categoria={Uri.EscapeDataString(Categoria)}");
        if (!string.IsNullOrWhiteSpace(Busca)) partes.Add($"busca={Uri.EscapeDataString(Busca)}");
        if (Promocao) partes.Add("promocao=true");
        if (PrecoMin is not null) partes.Add($"precoMin={PrecoMin}");
        if (PrecoMax is not null) partes.Add($"precoMax={PrecoMax}");
        if (Ordem != OrdenacaoProdutos.Relevancia) partes.Add($"ordem={Ordem}");
        if (Pagina > 1) partes.Add($"pagina={Pagina}");

        return partes.Count == 0 ? "/Produtos" : "/Produtos?" + string.Join("&", partes);
    }
}
