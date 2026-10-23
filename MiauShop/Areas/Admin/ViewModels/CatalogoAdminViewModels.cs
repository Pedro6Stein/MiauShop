using System.ComponentModel.DataAnnotations;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Areas.Admin.ViewModels;

// ---------- Categorias ----------

public record CategoriaLinha(Categoria Categoria, int QuantidadeProdutos);

public class CategoriaFormulario
{
    public int Id { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Use entre 2 e 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    [StringLength(300)]
    public string? Descricao { get; set; }

    [Display(Name = "Ícone")]
    [Required(ErrorMessage = "Escolha um ícone.")]
    public string Icone { get; set; } = string.Empty;

    [Display(Name = "Ordem no menu")]
    [Range(0, 99, ErrorMessage = "Use um número de 0 a 99.")]
    public int Ordem { get; set; }

    [Display(Name = "Ativa (aparece na loja)")]
    public bool Ativa { get; set; } = true;

    // Ícones oferecidos no formulário (Bootstrap Icons). Só estes são aceitos pelo servidor.
    public static readonly (string Classe, string Nome)[] IconesDisponiveis =
    [
        ("bi-balloon-heart", "Brinquedo"), ("bi-cup-hot", "Comida"), ("bi-stars", "Roupinha"),
        ("bi-house-heart", "Casinha"), ("bi-heart", "Coração"), ("bi-gift", "Presente"),
        ("bi-droplet", "Água"), ("bi-scissors", "Higiene"), ("bi-capsule", "Saúde"),
        ("bi-bag-heart", "Sacola"), ("bi-tag", "Etiqueta"), ("bi-moon-stars", "Descanso")
    ];
}

// ---------- Fornecedores ----------

public record FornecedorLinha(Fornecedor Fornecedor, int QuantidadeProdutos);

public class FornecedorFormulario
{
    public int Id { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Use entre 2 e 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Prazo de envio (dias úteis)")]
    [Range(1, 60, ErrorMessage = "Use um prazo de 1 a 60 dias.")]
    public int PrazoEnvioDias { get; set; } = 5;

    [Display(Name = "Contato")]
    [StringLength(200)]
    public string? Contato { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;
}

// ---------- Produtos ----------

public class ProdutosListaAdminViewModel
{
    public List<Produto> Produtos { get; init; } = [];
    public List<Categoria> Categorias { get; init; } = [];
    public string? Busca { get; init; }
    public int? CategoriaId { get; init; }
    public bool? Ativo { get; init; }
    public int Pagina { get; init; }
    public int TotalPaginas { get; init; }
    public int Total { get; init; }

    public string Url(int pagina)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(Busca)) partes.Add($"busca={Uri.EscapeDataString(Busca)}");
        if (CategoriaId is not null) partes.Add($"categoriaId={CategoriaId}");
        if (Ativo is not null) partes.Add($"ativo={Ativo.Value.ToString().ToLowerInvariant()}");
        if (pagina > 1) partes.Add($"pagina={pagina}");
        return "/Admin/Produtos" + (partes.Count > 0 ? "?" + string.Join("&", partes) : "");
    }
}

public class ProdutoFormulario
{
    public int Id { get; set; }
    public uint Versao { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Use entre 3 e 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Descrição curta (aparece no card)")]
    [Required(ErrorMessage = "Informe a descrição curta.")]
    [StringLength(200)]
    public string DescricaoCurta { get; set; } = string.Empty;

    [Display(Name = "Descrição completa")]
    [Required(ErrorMessage = "Informe a descrição.")]
    [StringLength(4000)]
    public string Descricao { get; set; } = string.Empty;

    [Display(Name = "Categoria")]
    [Required(ErrorMessage = "Escolha a categoria.")]
    public int? CategoriaId { get; set; }

    [Display(Name = "Fornecedor")]
    [Required(ErrorMessage = "Escolha o fornecedor.")]
    public int? FornecedorId { get; set; }

    [Display(Name = "Para")]
    public Animal Animal { get; set; }

    [Display(Name = "Preço (R$)")]
    [Required(ErrorMessage = "Informe o preço.")]
    public decimal? Preco { get; set; }

    [Display(Name = "Preço promocional (R$, opcional)")]
    public decimal? PrecoPromocional { get; set; }

    [Display(Name = "Destaque na página inicial")]
    public bool Destaque { get; set; }

    [Display(Name = "Ativo (à venda na loja)")]
    public bool Ativo { get; set; } = true;

    // Caminho do próprio site (/img/...) ou link https. Nada de "javascript:" ou outros esquemas.
    [Display(Name = "URL da imagem")]
    [Required(ErrorMessage = "Informe a URL da imagem.")]
    [StringLength(500)]
    [RegularExpression(@"^(/img/[\w\-/]+\.(webp|png|jpe?g)|https://\S+)$",
        ErrorMessage = "Use um caminho do site (/img/...) ou um link começando com https://.")]
    public string ImagemUrl { get; set; } = string.Empty;

    [Display(Name = "Descrição da imagem (acessibilidade)")]
    [Required(ErrorMessage = "Descreva a imagem.")]
    [StringLength(200)]
    public string ImagemTexto { get; set; } = string.Empty;
}

public class VariacaoFormulario
{
    public int Id { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome da variação.")]
    [StringLength(60)]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Ajuste de preço (R$)")]
    [Range(0, 9999, ErrorMessage = "O ajuste vai de R$ 0 a R$ 9.999.")]
    public decimal AjustePreco { get; set; }

    [Display(Name = "Ativa")]
    public bool Ativa { get; set; } = true;
}

public class ProdutoEdicaoViewModel
{
    public required ProdutoFormulario Formulario { get; init; }
    public string? Slug { get; init; }
    public List<Categoria> Categorias { get; init; } = [];
    public List<Fornecedor> Fornecedores { get; init; } = [];
    public List<ProdutoVariacao> Variacoes { get; init; } = [];
    public bool Novo => Formulario.Id == 0;
}
